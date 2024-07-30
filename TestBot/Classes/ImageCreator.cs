using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace PokemonGame.Classes
{
    public class ImageCreator
    {
        public async Task<Stream> GenerateRoomImage(List<Player> players)
        {
            var sortedPlayers = players.OrderByDescending(p => p.Wins).ToList();

            // Define scale modifier locally
            const float scaleModifier = 3f; // Scale modifier to reduce size by about a third

            // Define base dimensions with scale modifier
            int baseWidth = (int)(1000 * scaleModifier); // Reduced width
            int baseHeight = (int)(10 * scaleModifier); // Base height for the image

            // Calculate height based on the number of players
            int playerHeight = (int)(320 * scaleModifier); // Height reserved for each player
            int roomBannerHeight = (int)(150 * scaleModifier); // Height for the "ROOM" banner

            // Calculate the total height of the image
            int height = baseHeight + (sortedPlayers.Count * playerHeight) + roomBannerHeight;
            int width = baseWidth; // Fixed width

            // Minimum height check
            if (height < baseHeight + roomBannerHeight + playerHeight)
            {
                height = baseHeight + roomBannerHeight + playerHeight;
            }

            using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
            {
                SKCanvas canvas = surface.Canvas;

                // Draw a red-to-white gradient background
                using (var paint = new SKPaint())
                {
                    paint.Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(0, height),
                        new SKColor[] { SKColors.Red, SKColors.White },
                        null,
                        SKShaderTileMode.Repeat);

                    canvas.DrawRect(0, 0, width, height, paint);
                }

                // Draw the "ROOM" banner at the top
                using (var paint = new SKPaint())
                {
                    paint.Color = SKColors.Black;
                    canvas.DrawRect(0, 0, width, roomBannerHeight, paint);

                    paint.Color = SKColors.White;
                    paint.TextSize = 120 * scaleModifier; // Adjust text size with scale modifier
                    paint.IsAntialias = true;
                    paint.TextAlign = SKTextAlign.Center;
                    canvas.DrawText("ROOM", width / 2, roomBannerHeight / 2 + 40 * scaleModifier, paint);
                }

                // Draw black shapes and player data for each player
                int playerStartY = roomBannerHeight;
                foreach (var player in sortedPlayers)
                {
                    // Draw black square for avatar
                    using (var paint = new SKPaint())
                    {
                        paint.Color = SKColors.Black;
                        int avatarSize = (int)(200 * scaleModifier); // Scaled avatar size
                        canvas.DrawRect(20 * scaleModifier, playerStartY + 20 * scaleModifier, avatarSize, avatarSize, paint);

                        // Load and draw avatar image
                        if (!string.IsNullOrEmpty(player.AvatarURL))
                        {
                            using (var response = await StaticHTTPClient.httpClient.GetAsync(player.AvatarURL))
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var avatar = SKBitmap.Decode(stream))
                            {
                                canvas.DrawBitmap(avatar, new SKRect(20 * scaleModifier, playerStartY + 20 * scaleModifier, 20 * scaleModifier + avatarSize, playerStartY + 20 * scaleModifier + avatarSize));
                            }
                        }

                        // Draw long black round rectangle for username
                        int usernameRectWidth = width - (int)(320 * scaleModifier) - (int)(160 * scaleModifier); // Adjusted width for username box
                        int usernameRectHeight = (int)(80 * scaleModifier); // Scaled height of username box
                        float usernameRectX = avatarSize + (int)(60 * scaleModifier);
                        float usernameRectY = playerStartY + 20 * scaleModifier + avatarSize / 2 - usernameRectHeight / 2;
                        canvas.DrawRoundRect(usernameRectX, usernameRectY, usernameRectWidth, usernameRectHeight, 20 * scaleModifier, 20 * scaleModifier, paint);

                        // Draw the username text
                        paint.Color = SKColors.White;
                        paint.TextSize = 50 * scaleModifier; // Adjust text size as needed
                        paint.TextAlign = SKTextAlign.Center;
                        float usernameTextX = usernameRectX + usernameRectWidth / 2;
                        float usernameTextY = usernameRectY + usernameRectHeight / 2 + (paint.TextSize / 4); // Adjust vertical alignment
                        canvas.DrawText(player.User.Username, usernameTextX, usernameTextY, paint);

                        // Draw small black square for points
                        int pointsSize = (int)(120 * scaleModifier); // Scaled points box size
                        paint.Color = SKColors.Black;
                        float pointsRectX = width - pointsSize - (int)(40 * scaleModifier);
                        float pointsRectY = playerStartY + 20 * scaleModifier + avatarSize / 2 - pointsSize / 2;
                        canvas.DrawRect(pointsRectX, pointsRectY, pointsSize, pointsSize, paint);

                        // Draw the points text
                        paint.Color = SKColors.White;
                        paint.TextSize = 60 * scaleModifier; // Adjust text size as needed
                        paint.TextAlign = SKTextAlign.Center;
                        float pointsTextX = pointsRectX + pointsSize / 2;
                        float pointsTextY = pointsRectY + pointsSize / 2 + (paint.TextSize / 4); // Adjust vertical alignment
                        canvas.DrawText(player.Wins.ToString(), pointsTextX, pointsTextY, paint);
                    }

                    playerStartY += playerHeight;
                }

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    data.SaveTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream;
                }
            }
        }

        public async Task<Stream> GenerateResponseImage(List<string> imageUrls)
        {

            // List to hold the SKBitmap objects for each image
            List<SKBitmap> bitmaps = new List<SKBitmap>();

            // Load each image from the file path
            foreach (string path in imageUrls)
            {
                using var imageStream = File.OpenRead(path);
                var bitmap = SKBitmap.Decode(imageStream);
                bitmaps.Add(bitmap);
            }

            // Calculate the width and total height for the final image
            int width = bitmaps.Max(b => b.Width);
            int totalHeight = bitmaps.Sum(b => b.Height);

            // Create a new bitmap for the final image
            using var finalBitmap = new SKBitmap(width, totalHeight);
            using var canvas = new SKCanvas(finalBitmap);

            // Set the initial y-coordinate
            int yOffset = 0;

            // Draw each bitmap onto the final bitmap
            foreach (var bitmap in bitmaps)
            {
                canvas.DrawBitmap(bitmap, new SKPoint(0, yOffset));
                yOffset += bitmap.Height;
            }

            // Create an SKImage from the SKBitmap
            using var image = SKImage.FromBitmap(finalBitmap);

            // Encode the SKImage to a PNG and return it as a stream
            var encodedData = image.Encode(SKEncodedImageFormat.Png, 100); // Renamed to encodedData
            var outputStream = new MemoryStream(); // Renamed to outputStream
            encodedData.SaveTo(outputStream);
            outputStream.Position = 0;

            return outputStream;
        }

        public async Task<Stream> GeneratePromptImage(Prompt prompt)
        {
            // Load the background image
            string imagePath = Path.Combine(TextData.localImageUrl, "#Prompt Card.png"); // Update with your file path
            using (var backgroundImage = SKBitmap.Decode(imagePath))
            {
                // Create a new image info with the same dimensions as the background
                var info = new SKImageInfo(backgroundImage.Width, backgroundImage.Height);
                using (var surface = SKSurface.Create(info))
                {
                    var canvas = surface.Canvas;

                    // Draw the background image
                    canvas.DrawBitmap(backgroundImage, 0, 0);

                    // Define the text paint
                    var paint = new SKPaint
                    {
                        Color = SKColors.Black,
                        TextSize = 55, // Larger text size
                        IsAntialias = true,
                        Typeface = SKTypeface.FromFile(Path.Combine(TextData.localFontUrl,
                        "Sarpanch-Black.otf"))
                    };

                    // Prepare the text and convert to all caps
                    string wrappedText = prompt.promptText.ToUpper() + "\n\n" + 
                        prompt.responsesNeeded + " response(s)";
                    float maxWidth = info.Width - 250; // Allow some padding on the sides
                    float yShift = 30; // Adjust this to move the text up or down

                    // Split the text into lines
                    string[] lines = SplitTextToFitWidth(wrappedText, maxWidth, paint);

                    // Calculate the total height of the text block
                    float textHeight = lines.Length * (paint.TextSize + 5);

                    // Calculate the starting Y position to center the text block
                    float yTextStart = (info.Height / 2) - (textHeight / 2) + yShift;

                    // Draw the wrapped text, centered horizontally and vertically
                    float xTextStart = 20;
                    float yText = yTextStart;
                    foreach (string line in lines)
                    {
                        float xText = (info.Width - paint.MeasureText(line)) / 2;
                        canvas.DrawText(line, xText, yText, paint);
                        yText += paint.TextSize + 5; // Move to the next line with some padding
                    }

                    // Create the final image
                    using (var image = surface.Snapshot())
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        // Convert the image to a stream
                        var stream = new MemoryStream();
                        data.SaveTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        return stream;
                    }
                }
            }
        }

        // Helper method to split text into lines that fit within the specified width
        private string[] SplitTextToFitWidth(string text, float maxWidth, SKPaint paint)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float spaceWidth = paint.MeasureText(" ");
            float lineWidth = 0;

            foreach (string word in words)
            {
                float wordWidth = paint.MeasureText(word);
                if (lineWidth + wordWidth > maxWidth)
                {
                    sb.Append('\n');
                    lineWidth = 0;
                }
                sb.Append(word);
                sb.Append(' ');
                lineWidth += wordWidth + spaceWidth;
            }

            return sb.ToString().Split('\n');
        }

    }
}
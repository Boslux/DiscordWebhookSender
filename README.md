# Discord RSS Automation Tool <img src="DiscordWebhookSender/app.ico" width="32" />

A simple, modern C# WPF desktop application that fetches the latest news from an RSS feed and sends it to a Discord channel via Webhook.

## Features

- **RSS Parsing**: Automatically extracts the latest Title, Description (scrubbed of HTML), and Link.
- **Custom Message**: Option to add a custom message or ping before the news title.
- **Description Toggle**: Checkbox to include or exclude the original site description.
- **Local Settings**: Securely save your Webhook URL and RSS Link locally (`user_settings.json` is git-ignored).
- **Modern UI**: Dark-themed WPF interface with a beautiful robotic icon.

## How to Install & Run

1.  **Download/Clone**: Clone this repository or download the source.
2.  **Open Project**: Open `DiscordWebhookSender.sln` in Visual Studio.
3.  **Run**:
    - Via Terminal: `dotnet run --project DiscordWebhookSender`
    - Or build the executable: `dotnet publish -c Release -o ./Publish`
4.  **Use**:
    - Enter your **RSS Feed URL** (e.g., `https://example.com/feed`).
    - Enter your **Discord Webhook URL**.
    - Click **Send Now**.

## Requirements

- .NET 9.0 (or .NET 6.0+)
- Windows OS (WPF)

## Security Note

This project does NOT store your Webhook URLs in any cloud database or source file. If you check "Save Settings", they are stored in a local `user_settings.json` file on your computer, which is excluded from git version control.

## License

MIT License

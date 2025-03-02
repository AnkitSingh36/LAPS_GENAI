# LinkedIn Content Automation

## Overview
Tired of manually posting on LinkedIn?
Creating engaging LinkedIn content consistently can be challenging. Posting at the right time, optimizing content, and maintaining engagement requires effort. What if we could automate LinkedIn posts with AI while ensuring personalized and meaningful content?

We built an AI-powered LinkedIn Automation Tool that generates, optimizes, and posts content using .NET, Selenium, Hugging Face AI, and Telegram integration. This eliminates the need for manual posting while ensuring high-quality, data-driven content for LinkedIn users.
  
## Features
- **Automated LinkedIn Posting**: Uses Selenium to log in and post content.
- **AI-Generated Content**: Content is dynamically generated using Hugging Face models.
- **Error Handling & Logging**: Ensures reliability and logs errors.
- **Telegram Notifications**: Sends updates on successful posts and failures.

## Prerequisites
### 1. Install Required Software
- **.NET SDK** (Latest version)
- **Google Chrome** (For Selenium automation)
- **Chrome WebDriver** (Match Chrome version)

### 2. Set Up Environment Variables
Create a `.env` file or set environment variables manually:
```
LINKEDIN_USERNAME=your_email
LINKEDIN_PASSWORD=your_password
HuggingFaceApiKey=APIKEY
TelegramChatId=Chat_Id
TelegramBotToken=Bot_Token
```

## Installation & Setup
### 1. Clone the Repository
```sh
git clone https://github.com/your-repo/linkedin-automation.git
cd linkedin-automation
```

### 2. Install Dependencies
```sh
dotnet restore
```

### 3. Run the Automation Locally
```sh
dotnet run
```

## How It Works
1. **Fetch AI-generated content** from Hugging Face.
2. **Log in to LinkedIn using Selenium** and navigate to the post page.
3. **Post the generated content** automatically.
4. **Send a Telegram notification** upon success or failure.

## Future Improvements
- Multi-account support.
- Improved AI content generation.
- Scheduling via local task scheduler.

## Contributing
Feel free to contribute! Open an issue or submit a pull request.

##Note: This project automates posting content on LinkedIn using Selenium. Due to GitHub blacklisting requests to LinkedIn, we have moved away from GitHub cron jobs and now run the automation locally.

## License
MIT License


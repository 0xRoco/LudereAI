# LudereAI - Context-Aware AI Gaming Assistant

The AI Gaming Assistant That Understands Your Game.

LudereAI utilizes computer vision to analyze automatically captured in-game screenshot to better understand the player's situation and provides personalized, contextual, real-time friendly responses
in an interactive chat interface with voice synthesis support.

## Key Features

- **Game Detection**: Automatically identifies the game you're playing
- **Screenshot Analysis**: Understands what's happening on your screen using computer vision
- **Context-Aware Responses**: Provides relevant tips and information based on your current game state
- **Chat Interface**: Natural conversation with an AI that understands gaming
- **Text-to-Speech**: Optional voice responses

## Preview

<img alt="LudereAI Preview" src="Docs/Preview1.jpg"/>
<img alt="LudereAI Preview" src="Docs/Preview2.jpg"/>
<img alt="LudereAI Preview" src="Docs/Preview3.jpg"/>

## Why I Built This

I built LudereAI to bridge the gap between static guides and generic chatbots by creating an assistant that “sees” and understands your gameplay.
Inspired by the idea of an "AI friend" (think the movie "Her"),
I wanted to make gaming, especially single-player experiences, more engaging and interactive.

## Technology Stack

- **Backend**: .NET 9 with C#
- **Frontend**: WPF
- **AI**: OpenAI API (with support for Gemini via OpenAI-compatible endpoints)
- **Voice Synthesis**: ElevenLabs API (with alternative support for self-hosted Piper)
- **Database**: MySQL for conversation history, user preferences and authentication

## Setup and Installation

### Prerequisites

- Windows 10 or later
- .NET 9.0 SDK or later
- MySQL Server
- OpenAI or Gemini API key
- ElevenLabs API key (or self-hosted Piper for text-to-speech), both optional

### Configuration

1. Clone the repository:
   ```
   git clone https://github.com/0xRoco/LudereAI.git
   ```

2. Copy the example configuration file in the API project:
   ```
   cp appsettings.json.example appsettings.json
   ```

3. Edit `appsettings.json` and add your API keys:
   ```json
   {
     "AI": {
       "Name": "OpenAI",
       "ApiKey": "your-openai-api-key",
       "Model": "gpt-4o-mini",
       "Endpoint": "https://api.openai.com/v1/"
     },
     "ElevenLabs": {
       "ApiKey": "your-elevenlabs-api-key",
       "Voice": "VOICE_ID"
     },
     "Piper": {
       "ApiKey": "API_KEY",
       "Voice": "VOICE_ID",
       "Endpoint": "PIPER_ENDPOINT"
     }
   }
   ```
   - Supports any AI accessible via the OpenAI library, including Gemini.
   - Prioritizes the Piper API if available; otherwise, uses ElevenLabs, or none if both are missing.

4. Configure the MySQL database connection:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;port=3306;database=LudereAI;user=YOUR_USERNAME;password=YOUR_PASSWORD;"
     }
   }
   ```

### MySQL Database Setup

1. Install MySQL Server if you haven't already:
   - [MySQL Community Downloads](https://dev.mysql.com/downloads/installer/)
   - Or use Docker.

2. Create a database user with appropriate permissions:
   ```sql
   CREATE USER 'your_username'@'localhost' IDENTIFIED BY 'your_password';
   CREATE DATABASE LudereAI;
   GRANT ALL PRIVILEGES ON LudereAI.* TO 'your_username'@'localhost';
   FLUSH PRIVILEGES;
   ```

3. The backend server will automatically create the necessary tables on first.

### Running the Application

1. Build the solution:
   ```
   dotnet build
   ```
   
2. Run the backend server:
   ```
    dotnet run --project LudereAI.API
    ```

3. Run the WPF client:
   ```
   dotnet run --project LudereAI.WPF
   ```

## Challenges, and Future Directions

### Current Challenges

- **Response Time**: Vision API processing can take several seconds, which isn't ideal for fast-paced games
- **Context Understanding**: The AI sometimes misinterprets complex game states or UI elements
- **Game Coverage**: Limited knowledge of niche or very new games
- **Resource Usage**: Screenshot processing can be resource-intensive though no major issues were observed
### Possible *Future*™️ Improvements

- **Two-way voice chat**: Adding speech-to-text for hands-free interactions during gameplay
- **Knowledge Base**: Allow the AI to automatically update its knowledge base with new information related to a conversation
- **Game State Memory**: Improving the AI's ability to remember previous game states and player actions (Alongside Knowledge Base)
- **Unified Application**: Integrate the backend with the WPF client into a single all-in-one application for easier deployment.
## Project Status

LudereAI is currently in MVP (Minimum Viable Product) stage, a proof of concept with core functionality but there are many areas for improvement.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! If you have suggestions for improvements or new features, please open an issue or submit a pull request.

---

*Made with ❤️*

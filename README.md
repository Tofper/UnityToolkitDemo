# Unity UI Toolkit Demo

A demonstration project showcasing Unity's UI Toolkit implementation with a daily rewards system. This project uses modern Unity features including UI Toolkit and Localization.

## Features

- **Daily Rewards System**
  - Card-based reward interface
  - Multiple reward types (Coins, Gems, Tokens, XP)
  - Reroll functionality for unclaimed rewards
  - Premium and free reward tiers

- **Modern UI Implementation**
  - Built with Unity UI Toolkit
  - Responsive and scalable UI components
  - Custom UI controls and animations
  - USS styling system

- **Project Architecture**
  - MVVM (Model-View-ViewModel) pattern
  - Clean separation of concerns
  - Data-driven UI updates
  - Event-based communication

## Project Structure


Assets/
├── Fonts/                    # Custom fonts
├── Localization/            # Localization assets
├── Resources/               # Runtime resources
│   ├── *.uxml         # UI layout files
│   └── *.uss          # UI style files
├── Scenes/                 # Unity scenes
├── Scripts/                # C# source code
│   ├── Data/              # Data models and services
│   ├── Infrastructure/    # Core systems
│   ├── UI/               # UI components and screens
│   └── Utilities/        # Helper classes
├── Settings/              # Project settings
└── Shaders/              # Custom shaders
```

## Technical Details

### UI Components
- `DailyRewardCardControl`: Card component for displaying rewards
- `DailyRewardsScreen`: Main screen for the rewards system
- `CurrencyDisplayControl`: Currency display component
- `PremiumButtonControl`: Premium action button
- `RewardsRerollButton`: Reroll functionality button

### Data Structure
- `CardData`: Represents a single reward card
- `RewardData`: Contains reward type and amount
- `DailyRewardsViewModel`: Manages UI state and logic
- `DailyRewardsDataService`: Handles data operations

## Getting Started

1. Clone the repository
2. Open the project in Unity 2022.3 or later
3. Open the main scene in `Assets/Scenes/`
4. Press Play to test the daily rewards system

## Dependencies

- Unity 2022.3 or later
- UI Toolkit package
- Localization package

## UI Toolkit Features

- Custom UI Controls
- USS Styling
- UXML Layouts
- Event System
- Animations
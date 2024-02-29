
# Monster Trading Card Game

The aim of this Semester-project was to implement a HTTP/REST-based server serves as a platform for trading and battling in a magical card-game world. Players can engage in duels, manage their cards, and strategize their decks.

## Features
1. **User Registration and Credentials**
	- A user is a registered player with unique credentials (username and password).
	- These credentials grant access to the card-game world.
2. **Card Management**
	- Users can manage their collection of cards.
	- Each card has a name and multiple attributes, including damage and element type.
	- Cards fall into two categories: spell cards and monster cards.
	- The damage value of a card remains constant and does not change.
3. **Card Stacks**
	- Users maintain a stack of cards.
	- Stacks represent the collection of a user’s current cards.
	- Cards can be added or removed from the stack (e.g., through trading).
4. **Card Acquisition**
	- Users can acquire new cards by purchasing packages.
	- A package contains 5 cards and can be obtained from the server by spending 5 virtual coins.
	- Each user starts with 20 coins to buy 4 packages.
5. **Deck Building**
	- Users select their best 4 cards from their collection.
	- These chosen cards form their deck.
	- The deck is crucial for battles against other players.
6. **Battles**
	- A battle is a request to the server.
	- Users compete against each other using their currently defined decks.
	- Strategy, card selection, and timing are essential for victory.

## Getting Started

### Prerequisites
Please make sure you have the following set up:

1.  **.NET Core SDK**
    
    -   Make sure you have the .NET Core SDK installed on your system.
    -   We recommend using version 3.1.
2.  **Visual Studio or Visual Studio Code (Optional)**
    
    -   While not mandatory, having Visual Studio or Visual Studio Code can enhance your development experience

### Setting Up the Project
1.  **Clone the Repository**
    
    -   Clone this repository to your local machine using your preferred Git client or by downloading the ZIP file.
    -   Navigate to the project directory.

2. **Run the following commands**
```
dotnet restore
dotnet publish -c Release -o out
dotnet /MTCG_Rutschka/out/MTCG_Rutschka.dll
```

## Contributors

-   Rutschka Simon

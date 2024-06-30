using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZooManager
{
    public static class Game
    {
        public static int numCellsX { get; private set; } // Number of cells in the X dimension
        public static int numCellsY { get; private set; } // Number of cells in the Y dimension
        public static int riverColumn; // The column index where the river is located

        public static List<List<Zone>> animalZones = new List<List<Zone>>(); // 2D list to store animal zones
        private static Squirrel currentSquirrel; // The currently selected squirrel for the squirrel ability
        private static Tiger currentTiger; // The currently selected tiger
        public static List<Animal> squirrelTargets = new List<Animal>(); // List of targets for the squirrel ability

        public static Player Player1 { get; private set; } // Player 1 instance
        public static Player Player2 { get; private set; } // Player 2 instance
        public static Player CurrentPlayer { get; private set; } // The current player

        public enum GamePhase
        {
            Setup, // Game setup phase
            Placement, // Piece placement phase
            SquirrelAbility, // Squirrel ability phase
            Playing, // Regular gameplay phase
            GameOver // Game over phase
        }

        public static GamePhase CurrentPhase { get; set; } // The current game phase

        private static Animal selectedAnimal = null; // The currently selected animal

        public static event Action GameStateChanged; // Event triggered when the game state changes

        public enum MoveType
        {
            Move, // Move action
            Attack // Attack action
        }

        public static void SetUpGame()
        {
            CurrentPhase = GamePhase.Setup; // Set the game phase to setup
        }

        public static void SetBoardSize(int size)
        {
            if (size != 5 && size != 7)
            {
                throw new ArgumentException("Board size must be 5 or 7"); // Board size must be 5 or 7
            }
            numCellsX = size; // Set the number of cells in the X dimension
            numCellsY = size; // Set the number of cells in the Y dimension
            riverColumn = size / 2; // Set the column index where the river is located
        }

        // Modified: Updated StartGame method to enter the Placement phase
        public static void StartGame()
        {
            if (numCellsX == 0 || numCellsY == 0)
            {
                throw new InvalidOperationException("Board size not set"); // Board size must be set before starting the game
            }

            InitializeBoard(); // Initialize the board
            InitializePlayers(); // Initialize the players
            PlaceKings(); // Place kings
            CurrentPlayer = Player1; // Player 1 starts placing pieces
            CurrentPhase = GamePhase.Placement; // Enter the piece placement phase
            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        private static void InitializeBoard()
        {
            animalZones.Clear(); // Clear the animal zones list

            for (var y = 0; y < numCellsY; y++)
            {
                List<Zone> rowList = new List<Zone>();
                for (var x = 0; x < numCellsX; x++)
                {
                    bool isRiver = (x == riverColumn); // Check if the current cell is a river cell
                    rowList.Add(new Zone(x, y, null, isRiver)); // Add a new zone to the row
                }
                animalZones.Add(rowList); // Add the row to the animal zones list
            }
        }

        private static void InitializePlayers()
        {
            Player1 = new Player(1); // Create Player 1 instance
            Player2 = new Player(2); // Create Player 2 instance

            List<string> animalTypes = new List<string> { "Leopard", "Tiger", "Mouse", "Squirrel" }; // List of available animal types
            Random random = new Random();

            for (int i = 0; i < 2; i++)
            {
                int index = random.Next(animalTypes.Count); // Randomly select an animal type
                Player1.AssignAnimalType(animalTypes[index]); // Assign the animal type to Player 1
                animalTypes.RemoveAt(index); // Remove the assigned animal type from the list

                index = random.Next(animalTypes.Count); // Randomly select another animal type
                Player2.AssignAnimalType(animalTypes[index]); // Assign the animal type to Player 2
                animalTypes.RemoveAt(index); // Remove the assigned animal type from the list
            }

            Player1.AssignAnimalType("CatKing"); // Assign the CatKing to Player 1
            Player2.AssignAnimalType("MouseKing"); // Assign the MouseKing to Player 2
        }

        private static void PlaceKings()
        {
            PlaceKing(Player1, 0); // Place Player 1's king in the first column
            PlaceKing(Player2, numCellsX - 1); // Place Player 2's king in the last column
        }

        private static void PlaceKing(Player player, int column)
        {
            string kingType = player.Id == 1 ? "CatKing" : "MouseKing"; // Determine the king type based on the player ID
            Animal king = CreateAnimal(kingType, player); // Create the king animal
            int row = numCellsY / 2; // King is placed in the middle row
            Zone zone = animalZones[row][column]; // Get the zone where the king should be placed
            zone.occupant = king; // Set the occupant of the zone to the king
            king.location = zone.location; // Set the location of the king
            player.AddPiece(king); // Add the king to the player's pieces
            player.King = king; // Set the king for the player
        }

        // New: Added method to switch the current player
        public static void SwitchCurrentPlayer()
        {
            CurrentPlayer = (CurrentPlayer == Player1) ? Player2 : Player1; // Switch the current player
            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        // New: AI random placement method
        public static void AIPlaceAnimals(Player player)
        {
            List<string> availableTypes = GetAvailableAnimalTypes(player); // Get the available animal types for the player
            int piecesToPlace = numCellsX <= 5 ? 4 : 7; // Determine the number of pieces to place based on the board size

            // Modified: Consider the number of already placed non-King pieces
            int placedPieces = player.Pieces.Count(p => !(p is CatKing) && !(p is MouseKing)); // Count the number of non-King pieces already placed

            for (int i = placedPieces; i < piecesToPlace + 1; i++)
            {
                string animalType = availableTypes[new Random().Next(availableTypes.Count)]; // Randomly select an animal type
                Point location;
                do
                {
                    int x = player.Id == 1 ? new Random().Next(0, riverColumn) : new Random().Next(riverColumn + 1, numCellsX); // Determine the valid X range based on the player ID
                    int y = new Random().Next(0, numCellsY); // Randomly select a Y coordinate
                    location = new Point { x = x, y = y }; // Create a new Point with the selected coordinates
                } while (!IsValidPlacement(player, location)); // Keep generating random locations until a valid one is found

                PlaceAnimal(player, animalType, location); // Place the animal at the selected location
            }

            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        public static List<string> GetAvailableAnimalTypes(Player player)
        {
            return player.AssignedAnimalTypes.Where(type => type != "CatKing" && type != "MouseKing").ToList(); // Get a list of available animal types, excluding CatKing and MouseKing
        }

        // Modified: Updated PlaceAnimal method to consider board size
        public static bool PlaceAnimal(Player player, string animalType, Point location)
        {
            if (!IsValidPlacement(player, location))
            {
                Console.WriteLine($"Invalid placement at ({location.x}, {location.y})"); // Output an error message if the placement is invalid
                return false;
            }

            int maxPieces = numCellsX <= 5 ? 4 : 7; // Determine the maximum number of pieces based on the board size
            if (player.Pieces.Count(p => !(p is CatKing) && !(p is MouseKing)) >= maxPieces - 1)
            {
                Console.WriteLine($"Player {player.Id} has reached the maximum number of pieces"); // Output a message if the player has reached the maximum number of pieces
                return false;
            }

            Animal animal = CreateAnimal(animalType, player); // Create a new animal of the specified type for the player
            Zone zone = animalZones[location.y][location.x]; // Get the zone at the specified location
            zone.occupant = animal; // Set the occupant of the zone to the new animal
            animal.location = location; // Set the location of the animal
            player.AddPiece(animal); // Add the animal to the player's pieces

            Console.WriteLine($"Player {player.Id} placed {animalType} at ({location.x}, {location.y})"); // Output a message indicating the successful placement
            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
            return true;
        }

        public static bool IsValidPlacement(Player player, Point location)
        {
            int startColumn = player.Id == 1 ? 0 : (riverColumn + 1); // Determine the start column based on the player ID
            int endColumn = player.Id == 1 ? (riverColumn - 1) : (numCellsX - 1); // Determine the end column based on the player ID

            Console.WriteLine($"Checking location ({location.x}, {location.y}) for player {player.Id}"); // Output a message indicating the location being checked
            Console.WriteLine($"Valid range: {startColumn} to {endColumn}"); // Output the valid range of columns for the player

            if (location.x < startColumn || location.x > endColumn)
            {
                Console.WriteLine($"Location ({location.x}, {location.y}) is out of valid range for player {player.Id}"); // Output an error message if the location is out of range
                return false;
            }

            if (animalZones[location.y][location.x].occupant != null)
            {
                Console.WriteLine($"Location ({location.x}, {location.y}) is already occupied"); // Output an error message if the location is already occupied
                return false;
            }

            Console.WriteLine($"Location ({location.x}, {location.y}) is a valid placement"); // Output a message indicating that the location is valid
            return true;
        }

        private static Animal CreateAnimal(string animalType, Player owner)
        {
            Animal.Faction faction = owner.Id == 1 ? Animal.Faction.Cat : Animal.Faction.Mouse; // Determine the faction based on the player ID
            return AnimalFactory.CreateAnimal(animalType, $"{animalType}_{owner.Id}", faction); // Create a new animal of the specified type with a unique name and faction
        }

        private static Zone GetRandomEmptyZone(int startRow, int endRow, int startColumn, int endColumn)
        {
            Random random = new Random();
            List<Zone> emptyZones = animalZones
                .Skip(startRow).Take(endRow - startRow + 1) // Get the rows within the specified range
                .SelectMany(row => row.Skip(startColumn).Take(endColumn - startColumn + 1)) // Get the zones within the specified range
                .Where(zone => zone.occupant == null && !zone.isRiver) // Filter out occupied zones and river zones
                .ToList();

            if (emptyZones.Count == 0)
            {
                throw new InvalidOperationException("No available empty zones"); // Throw an exception if no empty zones are available
            }

            return emptyZones[random.Next(emptyZones.Count)]; // Return a random empty zone from the list
        }

        public static bool MoveAnimal(Animal animal, Point destination)
        {
            if (!IsValidMove(animal, destination))
            {
                return false; // Return false if the move is invalid
            }

            bool moveSuccess = animal.Move(destination); // Attempt to move the animal

            if (moveSuccess)
            {
                ApplyInteractionEffects(animal); // Apply interaction effects after a successful move
            }

            UpdateGameState(); // Update the game state
            return moveSuccess;
        }

        public static bool IsValidMove(Animal animal, Point destination)
        {
            if (!animal.CanMoveAndAttackThisTurn)
            {
                return false; // Return false if the animal cannot move and attack this turn
            }

            int distance = Math.Abs(animal.location.x - destination.x) + Math.Abs(animal.location.y - destination.y);
            if (distance > animal.GetModifiedMoveDistance() || distance == 0)
            {
                return false; // Return false if the distance is greater than the animal's modified move distance or the destination is the same as the current location
            }

            bool isCrossingRiver = IsCrossingRiver(animal.location, destination);
            if (isCrossingRiver)
            {
                if (animal is Leopard && ((Leopard)animal).CanCrossRiverFreely)
                {
                    if (distance > animal.GetModifiedMoveDistance() || distance == 0)
                    {
                        return false; // Return false if the distance is greater than the animal's modified move distance or the destination is the same as the current location
                    }
                }
                else
                {
                    if (!IsValidRiverCrossing(animal.location, destination))
                    {
                        return false; // Return false if the river crossing is invalid
                    }
                }
            }

            if (!IsWithinBoard(destination))
            {
                return false; // Return false if the destination is outside the board
            }

            Zone destinationZone = animalZones[destination.y][destination.x];
            if (destinationZone.isRiver && !(animal is Leopard && ((Leopard)animal).CanCrossRiverFreely))
            {
                return false; // Return false if the destination is a river zone and the animal cannot cross rivers freely
            }

            if (destinationZone.occupant != null && destinationZone.occupant.Owner == animal.Owner)
            {
                return false; // Return false if the destination is occupied by a friendly piece
            }

            if (animal.location.x != destination.x && animal.location.y != destination.y)
            {
                return false; // Return false if the move is not a straight line
            }

            if (!IsPathClear(animal.location, destination))
            {
                return false; // Return false if the path between the current location and the destination is not clear
            }

            return true;
        }

        private static bool IsValidRiverCrossing(Point start, Point end)
        {
            if (!IsCrossingRiver(start, end))
            {
                return false; // Return false if the move does not cross the river
            }

            if (start.y != end.y)
            {
                return false; // Return false if the move is not horizontal
            }

            return end.x == riverColumn + 1 || end.x == riverColumn - 1; // Return true if the destination is adjacent to the river column
        }

        private static bool IsPathClear(Point start, Point end)
        {
            int dx = Math.Sign(end.x - start.x);
            int dy = Math.Sign(end.y - start.y);
            Point current = new Point { x = start.x + dx, y = start.y + dy };

            while (current.x != end.x || current.y != end.y)
            {
                if (animalZones[current.y][current.x].occupant != null)
                {
                    return false; // Return false if the path is blocked by another piece
                }
                current.x += dx;
                current.y += dy;
            }

            return true; // Return true if the path is clear
        }

        private static bool IsWithinBoard(Point point)
        {
            return point.x >= 0 && point.x < numCellsX && point.y >= 0 && point.y < numCellsY; // Check if the point is within the board boundaries
        }

        public static bool IsCrossingRiver(Point start, Point end)
        {
            return (start.x <= riverColumn && end.x > riverColumn) || (start.x > riverColumn && end.x <= riverColumn); // Check if the move crosses the river
        }

        public static List<Point> GetValidMoves(Animal animal)
        {
            List<Point> validMoves = new List<Point>();
            int moveDistance = animal.GetModifiedMoveDistance();

            for (int dx = -moveDistance; dx <= moveDistance; dx++)
            {
                for (int dy = -moveDistance; dy <= moveDistance; dy++)
                {
                    if (Math.Abs(dx) + Math.Abs(dy) > moveDistance) continue; // Skip if the distance is greater than the move distance
                    if (dx != 0 && dy != 0) continue; // Skip if the move is not a straight line

                    Point destination = new Point { x = animal.location.x + dx, y = animal.location.y + dy };
                    if (IsValidMove(animal, destination))
                    {
                        validMoves.Add(destination); // Add the destination to the list of valid moves
                    }
                }
            }
            return validMoves;
        }

        public static void DebugPrintValidMoves(Animal animal)
        {
            var moves = GetValidMoves(animal);
            Console.WriteLine($"Valid moves for {animal.name} at ({animal.location.x}, {animal.location.y}):"); // Output the valid moves for the animal
            foreach (var move in moves)
            {
                Console.WriteLine($"  ({move.x}, {move.y})");
            }
        }

        public static void ZoneClick(Zone clickedZone)
        {
            switch (CurrentPhase)
            {
                case GamePhase.Placement:
                    HandlePlacementClick(clickedZone); // Handle the click during the placement phase
                    break;
                case GamePhase.Playing:
                    HandlePlayingClick(clickedZone); // Handle the click during the playing phase
                    break;
                case GamePhase.SquirrelAbility:
                    HandleSquirrelAbilityClick(clickedZone); // Handle the click during the squirrel ability phase
                    break;
                // Possibly other phases...
                default:
                    Console.WriteLine($"Unhandled game phase: {CurrentPhase}"); // Output a message for unhandled game phases
                    break;
            }

            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        private static void HandlePlacementClick(Zone clickedZone)
        {
            // TODO: Implement placement phase logic
            if (IsValidPlacement(CurrentPlayer, clickedZone.location))
            {
                // Placement logic
                // PlaceAnimal(CurrentPlayer, selectedAnimalType, clickedZone.location);
            }
        }

        private static void HandlePlayingClick(Zone clickedZone)
        {
            if (selectedAnimal == null)
            {
                if (clickedZone.occupant is Animal animal && animal.Owner == CurrentPlayer)
                {
                    selectedAnimal = animal; // Select the animal if it belongs to the current player
                }
            }
            else
            {
                if (clickedZone.occupant == selectedAnimal)
                {
                    selectedAnimal = null; // Deselect the animal if clicked again
                }
                else if (MoveOrAttack(selectedAnimal, clickedZone.location))
                {
                    EndTurn(); // End the turn if the move or attack was successful
                }
                selectedAnimal = null; // Deselect the animal
            }

            if (CheckVictory())
            {
                CurrentPhase = GamePhase.GameOver; // Set the game phase to GameOver if a player has won
            }
        }

        private static void HandleSquirrelAbilityClick(Zone clickedZone)
        {
            if (clickedZone.occupant is Animal target && target.Owner != CurrentPlayer)
            {
                AddSquirrelTarget(target); // Add the target to the squirrel targets list
            }
        }

        public static void EndTurn()
        {
            // Check if the current turn should end
            bool shouldEndTurn = true;

            if (shouldEndTurn)
            {
                // Update effects for all animals
                foreach (var row in animalZones)
                {
                    foreach (var zone in row)
                    {
                        if (zone.occupant is Animal animal)
                        {
                            animal.UpdateEffects(); // Update the effects for each animal
                        }
                    }
                }

                // Switch current player
                CurrentPlayer = (CurrentPlayer == Player1) ? Player2 : Player1;

                // Reset move and attack status for all pieces of the current player
                foreach (var piece in CurrentPlayer.Pieces)
                {
                    piece.CanMoveAndAttackThisTurn = true;
                    piece.HasFledTurn = false;
                }

                // Add any logic that needs to be executed at the start of the turn here

                // Trigger the GameStateChanged event to notify the UI to update
                GameStateChanged?.Invoke();
            }
        }

        public static Animal GetSelectedAnimal()
        {
            return selectedAnimal; // Return the currently selected animal
        }

        public static bool IsAnimalSelected(Point location)
        {
            return selectedAnimal != null &&
                   selectedAnimal.location.x == location.x &&
                   selectedAnimal.location.y == location.y; // Check if an animal is selected at the given location
        }

        public static void ApplyInteractionEffects(Animal movedAnimal)
        {
            movedAnimal.DetectNearbyAnimals(); // Detect nearby animals for the moved animal
            movedAnimal.HandleInteractions(); // Handle interactions for the moved animal

            // Detect and handle interactions for nearby animals as well
            foreach (var nearbyAnimal in movedAnimal.NearbyAnimals)
            {
                nearbyAnimal.DetectNearbyAnimals(); // Detect nearby animals for each nearby animal
                nearbyAnimal.HandleInteractions(); // Handle interactions for each nearby animal
            }
        }

        private static void RemoveEffect(Animal target, Animal source)
        {
            if (target.EffectSource == source)
            {
                target.CurrentEffect = InteractionEffect.None;
                target.EffectSource = null;
                // Reset other effect-related properties...
            }
        }

        private static void ApplyEffect(Animal target, InteractionEffect effect, Animal source)
        {
            switch (effect)
            {
                case InteractionEffect.Attract:
                    target.ApplyEffect(effect, 1, source); // Apply the attract effect to the target
                    target.MoveDistanceModifier = 1; // Increase the move distance modifier for the target
                    Console.WriteLine($"{source.name} attracted {target.name}");
                    break;
                case InteractionEffect.Repel:
                    target.ApplyEffect(effect, 1, source); // Apply the repel effect to the target
                    target.MoveDistanceModifier = -1; // Decrease the move distance modifier for the target
                    Console.WriteLine($"{source.name} repelled {target.name}");
                    break;
                case InteractionEffect.Pursue:
                    source.CanAttackAgain = true; // Allow the source animal to attack again
                    Console.WriteLine($"{source.name} can attack {target.name} again");
                    break;
                case InteractionEffect.Flee:
                    target.ApplyEffect(effect, 0, source); // Apply the flee effect to the target
                    target.IsFleeingFrom = source; // Set the source animal as the one the target is fleeing from
                    Console.WriteLine($"{target.name} is fleeing from {source.name}");
                    break;
                case InteractionEffect.Intimidate:
                    if (new Random().Next(2) == 0) // 50% chance
                    {
                        target.ApplyEffect(InteractionEffect.Paralyze, 1, source); // Apply the paralyze effect to the target
                        target.IsParalyzed = true; // Set the target as paralyzed
                        Console.WriteLine($"{source.name} intimidated and paralyzed {target.name}");
                    }
                    else
                    {
                        Console.WriteLine($"{source.name} attempted to intimidate {target.name}, but failed"); // Output a message if the intimidate attempt failed
                    }
                    break;
                case InteractionEffect.Paralyze:
                    target.ApplyEffect(effect, 1, source); // Apply the paralyze effect to the target
                    target.IsParalyzed = true; // Set the target as paralyzed
                    Console.WriteLine($"{target.name} is paralyzed by {source.name}");
                    break;
            }
        }

        // Comment: New method to get the effect range
        public static int GetEffectRange(Animal source, Animal target)
        {
            InteractionEffect effect = GetInteractionEffect(source, target);
            switch (effect)
            {
                case InteractionEffect.Attract:
                case InteractionEffect.Repel:
                    return source is CatKing || source is MouseKing ? 2 : 1; // Return the effect range based on the source animal type
                case InteractionEffect.Pursue:
                    return source is Leopard ? 2 : 1; // Return the effect range based on the source animal type
                case InteractionEffect.Flee:
                    return target is Mouse ? 2 : 1; // Return the effect range based on the target animal type
                case InteractionEffect.Intimidate:
                    return 1; // Intimidate effect has a range of 1
                default:
                    return 0; // Return 0 for no effect
            }
        }

        // Modified: GetInteractionEffect method
        public static InteractionEffect GetInteractionEffect(Animal source, Animal target)
        {
            if (source is Leopard || source is Tiger)
            {
                if (target is Mouse || target is Squirrel)
                    return InteractionEffect.Pursue; // Leopards and Tigers pursue Mice and Squirrels
                if (source.GetType() == target.GetType())
                    return InteractionEffect.Repel; // Leopards and Tigers repel their own kind
            }
            else if (source is Mouse || source is Squirrel)
            {
                if (target is Leopard || target is Tiger)
                    return InteractionEffect.Flee; // Mice and Squirrels flee from Leopards and Tigers
                if (source.GetType() == target.GetType())
                    return InteractionEffect.Attract; // Mice and Squirrels attract their own kind
            }
            else if (source is CatKing)
            {
                if (target is Leopard || target is Tiger)
                    return InteractionEffect.Attract; // CatKing attracts Leopards and Tigers
                if (target is Mouse || target is Squirrel)
                    return InteractionEffect.Intimidate; // CatKing intimidates Mice and Squirrels
            }
            else if (source is MouseKing)
            {
                if (target is Leopard || target is Tiger)
                    return InteractionEffect.Repel; // MouseKing repels Leopards and Tigers
                if (target is Mouse || target is Squirrel)
                    return InteractionEffect.Attract; // MouseKing attracts Mice and Squirrels
            }
            return InteractionEffect.None; // Return None if no interaction effect applies
        }

        public static bool IsSeparatedByRiver(Animal animal1, Animal animal2)
        {
            return (animal1.location.x <= riverColumn && animal2.location.x > riverColumn) ||
                   (animal1.location.x > riverColumn && animal2.location.x <= riverColumn); // Check if the animals are separated by the river
        }

        private static bool CheckVictory()
        {
            bool player1KingDefeated = Player1.King == null || Player1.Pieces.Count <= 1; // Check if Player 1's king is defeated
            bool player2KingDefeated = Player2.King == null || Player2.Pieces.Count <= 1; // Check if Player 2's king is defeated

            if (player1KingDefeated)
            {
                Console.WriteLine("Player 2 wins! Player 1's king has been defeated."); // Output a message if Player 2 wins
                return true;
            }
            else if (player2KingDefeated)
            {
                Console.WriteLine("Player 1 wins! Player 2's king has been defeated."); // Output a message if Player 1 wins
                return true;
            }

            return false; // Return false if no player has won yet
        }

        public static bool UseSpecialAbility(Animal animal)
        {
            if (animal.UseSpecialAbility())
            {
                animal.CanMoveAndAttackThisTurn = !animal.ShouldEndTurnAfterSpecialAbility(); // Update the animal's move and attack status based on the special ability

                bool abilitySuccess = false;

                if (animal is Tiger tiger)
                {
                    abilitySuccess = tiger.UseSpecialAbility(); // Use the Tiger's special ability
                }
                else if (animal is Squirrel squirrel)
                {
                    squirrel.UseSpecialAbility(); // Use the Squirrel's special ability
                    abilitySuccess = true;
                }
                else if (animal is CatKing catKing)
                {
                    abilitySuccess = catKing.UseSpecialAbility(); // Use the CatKing's special ability
                }
                else if (animal is MouseKing mouseKing)
                {
                    abilitySuccess = mouseKing.UseSpecialAbility(); // Use the MouseKing's special ability
                }
                else if (animal is Leopard)
                {
                    abilitySuccess = true; // Leopard's ability always succeeds
                }

                if (abilitySuccess && animal.ShouldEndTurnAfterSpecialAbility())
                {
                    EndTurn(); // End the turn if the special ability was successful and the animal should end its turn
                }

                GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
                return abilitySuccess;
            }
            return false;
        }

        public static void SkipTurn()
        {
            EndTurn(); // End the current turn
            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        // TODO: Implement ExchangePieces method for the "assassination" and "defection" effects
        public static void ExchangePieces(Animal attacker, Animal defender)
        {
            // Implement the logic for "assassination" and "defection" effects
            if (attacker.Owner.ExchangeCount > 0 || defender.Owner.ExchangeCount > 0)
            {
                return; // Each player can only exchange once
            }

            // Exchange pieces
            Player attackerOwner = attacker.Owner;
            Player defenderOwner = defender.Owner;

            attackerOwner.RemovePiece(attacker);
            defenderOwner.RemovePiece(defender);

            attackerOwner.AddPiece(defender);
            defenderOwner.AddPiece(attacker);

            // Update positions
            Point tempLocation = attacker.location;
            attacker.location = defender.location;
            defender.location = tempLocation;

            // Update board
            animalZones[attacker.location.y][attacker.location.x].occupant = attacker;
            animalZones[defender.location.y][defender.location.x].occupant = defender;

            // Update factions
            attacker.SwitchFaction();
            defender.SwitchFaction();

            // Increase exchange count
            attackerOwner.ExchangeCount++;
            defenderOwner.ExchangeCount++;

            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        // New: Check if exchange can be triggered
        public static bool CanExchangePieces(Animal attacker, Animal defender)
        {
            // Check if the conditions for triggering are met
            bool isAssassinationCondition = (attacker is Tiger || attacker is Leopard) &&
                                            attacker.CurrentFaction == Animal.Faction.Mouse &&
                                            defender is CatKing;
            bool isDefectionCondition = attacker is Mouse &&
                                        attacker.CurrentFaction == Animal.Faction.Cat;

            return (isAssassinationCondition || isDefectionCondition) &&
                   attacker.Owner.ExchangeCount == 0 &&
                   defender.Owner.ExchangeCount == 0; // Return true if the conditions are met and neither player has exchanged pieces before
        }

        public static void RemoveAnimal(Animal animal)
        {
            Zone zone = animalZones[animal.location.y][animal.location.x]; // Get the zone where the animal is located
            zone.occupant = null; // Remove the animal from the zone
            animal.Owner.RemovePiece(animal); // Remove the animal from the owner's pieces
        }

        // New: Trigger exchange effect
        public static void TriggerExchangeEffect(Animal attacker, Animal defender)
        {
            if (CanExchangePieces(attacker, defender))
            {
                ExchangePieces(attacker, defender); // Exchange the pieces if the conditions are met
            }
        }

        // New: Reset game
        public static void ResetGame()
        {
            CurrentPhase = GamePhase.Setup; // Reset the game phase to Setup
            animalZones.Clear(); // Clear the animal zones
            Player1 = null; // Reset Player 1
            Player2 = null; // Reset Player 2
            CurrentPlayer = null; // Reset the current player
            selectedAnimal = null; // Reset the selected animal
            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        public static bool MoveOrAttack(Animal animal, Point destination)
        {
            if (!IsValidMove(animal, destination))
            {
                return false; // Return false if the move is invalid
            }

            Zone destinationZone = animalZones[destination.y][destination.x];

            if (destinationZone.occupant != null && destinationZone.occupant.Owner != animal.Owner)
            {
                // If the destination has an enemy piece, perform an attack
                ProcessAttack(animal, (Animal)destinationZone.occupant);
                return true;
            }

            // If the destination is empty, perform a move
            bool moveSuccess = animal.Move(destination);
            if (moveSuccess)
            {
                ApplyInteractionEffects(animal); // Apply interaction effects after a successful move
            }

            return moveSuccess;
        }

        public static void HandleFleeEffect(Animal animal)
        {
            if (animal.IsFleeingFrom == null)
            {
                return; // If the animal is not fleeing, return
            }

            // Set the maximum number of attempts to avoid potential infinite loops
            const int maxAttempts = 3;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                Point fleeDestination = GetFleeDestination(animal, animal.IsFleeingFrom);

                // If unable to flee (returning to the original position), end the flee
                if (fleeDestination.Equals(animal.location))
                {
                    break;
                }

                if (TryMoveAnimal(animal, fleeDestination))
                {
                    Console.WriteLine($"{animal.name} fled from {animal.IsFleeingFrom.name} to ({fleeDestination.x}, {fleeDestination.y})");
                    break; // Successfully moved, exit the loop
                }

                attempts++;
            }

            // Reset the fleeing status, whether successful or not
            animal.IsFleeingFrom = null;
            animal.HasFledTurn = true;

            // If the maximum number of attempts is reached without success, log an error
            if (attempts == maxAttempts)
            {
                Console.WriteLine($"{animal.name} failed to flee after {maxAttempts} attempts");
            }
        }

        private static bool TryMoveAnimal(Animal animal, Point destination)
        {
            try
            {
                return MoveAnimal(animal, destination); // Attempt to move the animal
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving {animal.name}: {ex.Message}"); // Log an error if an exception occurs
                return false;
            }
        }

        public static void ProcessAttack(Animal attacker, Animal defender)
        {
            attacker.Attack(defender); // Perform the attack

            // Handle global effects and complex logic

            if (defender.healthPoints <= 0)
            {
                RemoveAnimal(defender); // Remove the defender if its health points are depleted
            }

            // Apply interaction effects
            // ApplyInteractionEffects(attacker);
            // ApplyInteractionEffects(defender);

            // Check if the exchange effect is triggered
            //if (CanExchangePieces(attacker, defender))
            //{
            //TriggerExchangeEffect(attacker, defender);
            //}

            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        private static void UpdateGameState()
        {
            // Update effects for all animals
            foreach (var row in animalZones)
            {
                foreach (var zone in row)
                {
                    if (zone.occupant is Animal animal)
                    {
                        animal.UpdateEffects(); // Update the effects for each animal
                    }
                }
            }

            // Check victory conditions
            if (CheckVictory())
            {
                CurrentPhase = GamePhase.GameOver; // Set the game phase to GameOver if a player has won
                                                   // You can add logic here to display a victory message
            }

            // Check if the current turn should end
            bool shouldEndTurn = true;
            foreach (var piece in CurrentPlayer.Pieces)
            {
                if (piece is Animal animal && (animal.CanMoveAndAttackThisTurn || animal.CanAttackAgain))
                {
                    shouldEndTurn = false;
                    break;
                }
            }

            if (shouldEndTurn)
            {
                EndTurn(); // End the turn if no pieces can move or attack
            }

            // Trigger the GameStateChanged event to notify the UI to update
            GameStateChanged?.Invoke();
        }

        public static Point GetFleeDestination(Animal fleeing, Animal pursuer)
        {
            int dx = fleeing.location.x - pursuer.location.x;
            int dy = fleeing.location.y - pursuer.location.y;

            // Determine the primary direction to flee (horizontal or vertical)
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                // Flee horizontally
                int newX = fleeing.location.x + Math.Sign(dx);
                Point destination = new Point { x = newX, y = fleeing.location.y };
                if (IsValidFleeDestination(destination, fleeing))
                {
                    return destination;
                }
            }
            else
            {
                // Flee vertically
                int newY = fleeing.location.y + Math.Sign(dy);
                Point destination = new Point { x = fleeing.location.x, y = newY };
                if (IsValidFleeDestination(destination, fleeing))
                {
                    return destination;
                }
            }

            // If the primary direction is not valid, try the other direction
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                // Try vertical
                int newY = fleeing.location.y + Math.Sign(dy);
                Point destination = new Point { x = fleeing.location.x, y = newY };
                if (IsValidFleeDestination(destination, fleeing))
                {
                    return destination;
                }
            }
            else
            {
                // Try horizontal
                int newX = fleeing.location.x + Math.Sign(dx);
                Point destination = new Point { x = newX, y = fleeing.location.y };
                if (IsValidFleeDestination(destination, fleeing))
                {
                    return destination;
                }
            }

            // If no valid flee destination, return the current location
            return fleeing.location;
        }

        private static bool IsValidFleeDestination(Point destination, Animal animal)
        {
            if (!IsWithinBoard(destination))
            {
                return false; // Return false if the destination is outside the board
            }

            if (animalZones[destination.y][destination.x].occupant != null)
            {
                return false; // Return false if the destination is occupied
            }

            if (IsCrossingRiver(animal.location, destination) &&
                !(animal is Leopard && ((Leopard)animal).CanCrossRiverFreely))
            {
                return false; // Return false if the animal is crossing the river and cannot cross rivers freely
            }

            if (destination.x == riverColumn)
            {
                return false; // Return false if the destination is on the river column
            }

            return true; // Return true if the destination is valid
        }

        private static int GetDistance(Point a, Point b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y); // Calculate the distance between two points
        }

        private static Point ChooseBestFleeDestination(Animal defender, List<Point> possibleDestinations)
        {
            // Find all enemy animals
            var enemies = animalZones.SelectMany(row => row)
                                     .Where(zone => zone.occupant is Animal &&
                                                    zone.occupant.Owner != defender.Owner)
                                     .Select(zone => zone.occupant as Animal)
                                     .ToList();

            // Calculate the shortest distance to each enemy and choose the farthest destination
            return possibleDestinations.OrderByDescending(dest =>
                enemies.Min(enemy => GetDistance(dest, enemy.location))
            ).First();
        }

        // New: CatKing's rally ability implementation
        public static bool CatKingRally(CatKing catKing)
        {
            // Get friendly pieces that can move
            var friendlyPieces = CurrentPlayer.Pieces.Where(p => p != catKing && p.CanMoveAndAttackThisTurn).ToList();
            if (friendlyPieces.Count == 0)
            {
                Console.WriteLine("No friendly pieces available to move");
                return false;
            }

            // Select a friendly piece (simplified to random selection here)
            var selectedPiece = friendlyPieces[new Random().Next(friendlyPieces.Count)];

            // Get valid moves for the selected piece
            var validMoves = GetValidMoves(selectedPiece);
            if (validMoves.Count == 0)
            {
                Console.WriteLine("No valid moves for the selected piece");
                return false;
            }

            // Select a position to move to (simplified to random selection here)
            var destination = validMoves[new Random().Next(validMoves.Count)];

            // Move the piece
            selectedPiece.Move(destination);

            // Restore health points
            catKing.healthPoints = Math.Min(catKing.healthPoints + 5, 50); // Assuming maximum health is 50

            Console.WriteLine($"{catKing.name} used the rally ability, moving {selectedPiece.name} and restoring 5 health points");
            return true;
        }

        // New: MouseKing's revive ability implementation
        public static bool MouseKingRevive(MouseKing mouseKing)
        {
            // Get defeated friendly pieces
            var deadPieces = CurrentPlayer.DeadPieces;
            if (deadPieces.Count == 0)
            {
                Console.WriteLine("No friendly pieces available to revive");
                return false;
            }

            // Randomly select a defeated piece
            var revivedPiece = deadPieces[new Random().Next(deadPieces.Count)];

            // Find an empty adjacent spot to place the revived piece
            var emptyAdjacentSpot = GetEmptyAdjacentSpot(mouseKing.location);
            if (emptyAdjacentSpot == null)
            {
                Console.WriteLine("No empty adjacent spots to place the revived piece");
                return false;
            }

            // Revive the piece
            revivedPiece.healthPoints = 1;
            string animalType = char.ToUpper(revivedPiece.species[0]) + revivedPiece.species.Substring(1);
            // Modified: Added null check and type conversion
            if (PlaceAnimal(CurrentPlayer, animalType, emptyAdjacentSpot.Value))
            {
                CurrentPlayer.DeadPieces.Remove(revivedPiece);
                Console.WriteLine($"{mouseKing.name} used the revive ability, reviving {revivedPiece.name}");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to revive {revivedPiece.name}");
                return false;
            }
        }

        // Comment: This is the modified code that resolves the compilation errors

        private static Point? GetEmptyAdjacentSpot(Point location)
        {
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int newX = location.x + dx[i];
                int newY = location.y + dy[i];

                if (IsWithinBoard(new Point { x = newX, y = newY }) &&
                    animalZones[newY][newX].occupant == null)
                {
                    return new Point { x = newX, y = newY };
                }
            }

            return null; // Return null if no empty adjacent spot is found
        }

        public static void StartSquirrelAbility(Squirrel squirrel)
        {
            currentSquirrel = squirrel; // Set the current squirrel for the ability
            squirrelTargets.Clear(); // Clear the squirrel targets list
            CurrentPhase = GamePhase.SquirrelAbility; // Set the game phase to SquirrelAbility
            Console.WriteLine("Please select two targets to attack.");
            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        public static void AddSquirrelTarget(Animal target)
        {
            if (target != null && target.Owner != CurrentPlayer && squirrelTargets.Count < 2 && !squirrelTargets.Contains(target))
            {
                squirrelTargets.Add(target); // Add the target to the squirrel targets list
                Console.WriteLine($"Selected target: {target.name}");
                GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
            }
        }

        private static void ApplyDirectDamage(Animal source, Animal target, int damage)
        {
            target.TakeDamage(damage); // Apply direct damage to the target
            Console.WriteLine($"{source.name} dealt {damage} direct damage to {target.name}!");

            if (target.healthPoints <= 0)
            {
                Console.WriteLine($"{target.name} has been defeated!");
                RemoveAnimal(target); // Remove the target if its health points are depleted
            }
        }

        public static void ExecuteSquirrelAbility()
        {
            if (currentSquirrel == null || squirrelTargets.Count != 2)
            {
                Console.WriteLine("Unable to execute squirrel ability: Invalid squirrel or targets");
                return;
            }

            foreach (var target in squirrelTargets)
            {
                if (target != null)
                {
                    ApplyDirectDamage(currentSquirrel, target, currentSquirrel.attackPower); // Apply direct damage to the targets
                }
            }

            // Reset variables related to squirrel ability
            currentSquirrel = null;
            squirrelTargets.Clear();

            // End the current turn
            EndTurn();

            CurrentPhase = GamePhase.Playing; // Set the game phase back to Playing
            GameStateChanged?.Invoke(); // Trigger the GameStateChanged event
        }

        public static bool StartTigerAbility(Tiger tiger)
        {
            Console.WriteLine($"{tiger.name} used the roar ability!");

            // Get the enemy king
            Animal enemyKing = (tiger.Owner == Player1) ? Player2.King : Player1.King;

            if (enemyKing != null)
            {
                ApplyDirectDamage(tiger, enemyKing, tiger.attackPower); // Apply direct damage to the enemy king
                Console.WriteLine($"{tiger.name} dealt {tiger.attackPower} damage to {enemyKing.name}!");
                EndTurn(); // End the turn
                return true;
            }
            else
            {
                Console.WriteLine("The roar ability did not affect the enemy king!");
                return false;
            }
        }

        private static Point GetAdjacentLocation(Point location, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new Point { x = location.x, y = location.y - 1 }; // Return the location above the current location
                case Direction.Down:
                    return new Point { x = location.x, y = location.y + 1 }; // Return the location below the current location
                case Direction.Left:
                    return new Point { x = location.x - 1, y = location.y }; // Return the location to the left of the current location
                case Direction.Right:
                    return new Point { x = location.x + 1, y = location.y }; // Return the location to the right of the current location
                default:
                    throw new ArgumentException("Invalid direction"); // Throw an exception if an invalid direction is provided
            }
        }

        public enum Direction
        {
            Up, // Enumeration value for the up direction
            Down, // Enumeration value for the down direction
            Left, // Enumeration value for the left direction
            Right // Enumeration value for the right direction
        }
    }
}
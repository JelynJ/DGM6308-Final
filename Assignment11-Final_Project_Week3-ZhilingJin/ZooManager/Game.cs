using System;
using System.Collections.Generic;
using System.Linq;

namespace ZooManager
{
    public static class Game
    {
        public static int numCellsX { get; private set; }
        public static int numCellsY { get; private set; }
        public static int riverColumn;

        public static List<List<Zone>> animalZones = new List<List<Zone>>();
        private static Squirrel currentSquirrel;
        private static Tiger currentTiger;
        private static List<Animal> squirrelTargets = new List<Animal>();

        public static Player Player1 { get; private set; }
        public static Player Player2 { get; private set; }
        public static Player CurrentPlayer { get; private set; }

        public enum GamePhase
        {
            Setup,
            Placement,  // New: Piece placement phase
            SquirrelAbility,  // New: Squirrel ability phase
            Playing,
            GameOver
        }

        public static GamePhase CurrentPhase { get; set; }

        private static Animal selectedAnimal = null;

        public static event Action GameStateChanged;

        public enum MoveType
        {
            Move,
            Attack
        }

        public static void SetUpGame()
        {
            CurrentPhase = GamePhase.Setup;
        }

        public static void SetBoardSize(int size)
        {
            if (size != 5 && size != 7)
            {
                throw new ArgumentException("Board size must be 5 or 7");
            }
            numCellsX = size;
            numCellsY = size;
            riverColumn = size / 2;
        }

        // Modified: Updated StartGame method to enter the Placement phase
        public static void StartGame()
        {
            if (numCellsX == 0 || numCellsY == 0)
            {
                throw new InvalidOperationException("Board size not set");
            }

            InitializeBoard();
            InitializePlayers();
            PlaceKings();  // Place kings
            CurrentPlayer = Player1;  // Player 1 starts placing pieces
            CurrentPhase = GamePhase.Placement;  // Enter the piece placement phase
            GameStateChanged?.Invoke();
        }

        private static void InitializeBoard()
        {
            animalZones.Clear();

            for (var y = 0; y < numCellsY; y++)
            {
                List<Zone> rowList = new List<Zone>();
                for (var x = 0; x < numCellsX; x++)
                {
                    bool isRiver = (x == riverColumn);
                    rowList.Add(new Zone(x, y, null, isRiver));
                }
                animalZones.Add(rowList);
            }
        }

        private static void InitializePlayers()
        {
            Player1 = new Player(1);
            Player2 = new Player(2);

            List<string> animalTypes = new List<string> { "Leopard", "Tiger", "Mouse", "Squirrel" };
            Random random = new Random();

            for (int i = 0; i < 2; i++)
            {
                int index = random.Next(animalTypes.Count);
                Player1.AssignAnimalType(animalTypes[index]);
                animalTypes.RemoveAt(index);

                index = random.Next(animalTypes.Count);
                Player2.AssignAnimalType(animalTypes[index]);
                animalTypes.RemoveAt(index);
            }

            Player1.AssignAnimalType("CatKing");
            Player2.AssignAnimalType("MouseKing");
        }

        private static void PlaceKings()
        {
            PlaceKing(Player1, 0);  // Player 1's king is placed in the first column
            PlaceKing(Player2, numCellsX - 1);  // Player 2's king is placed in the last column
        }

        private static void PlaceKing(Player player, int column)
        {
            string kingType = player.Id == 1 ? "CatKing" : "MouseKing";
            Animal king = CreateAnimal(kingType, player);
            int row = numCellsY / 2;  // King is placed in the middle row
            Zone zone = animalZones[row][column];
            zone.occupant = king;
            king.location = zone.location;
            player.AddPiece(king);
            player.King = king;
        }

        // New: Added method to switch the current player
        public static void SwitchCurrentPlayer()
        {
            CurrentPlayer = (CurrentPlayer == Player1) ? Player2 : Player1;
            GameStateChanged?.Invoke();
        }

        // New: AI random placement method
        public static void AIPlaceAnimals(Player player)
        {
            List<string> availableTypes = GetAvailableAnimalTypes(player);
            int piecesToPlace = numCellsX <= 5 ? 4 : 7;

            // Modified: Consider the number of already placed non-King pieces
            int placedPieces = player.Pieces.Count(p => !(p is CatKing) && !(p is MouseKing));

            for (int i = placedPieces; i < piecesToPlace + 1; i++)
            {
                string animalType = availableTypes[new Random().Next(availableTypes.Count)];
                Point location;
                do
                {
                    int x = player.Id == 1 ? new Random().Next(0, riverColumn) : new Random().Next(riverColumn + 1, numCellsX);
                    int y = new Random().Next(0, numCellsY);
                    location = new Point { x = x, y = y };
                } while (!IsValidPlacement(player, location));

                PlaceAnimal(player, animalType, location);
            }

            GameStateChanged?.Invoke();
        }

        public static List<string> GetAvailableAnimalTypes(Player player)
        {
            return player.AssignedAnimalTypes.Where(type => type != "CatKing" && type != "MouseKing").ToList();
        }

        // Modified: Updated PlaceAnimal method to consider board size
        public static bool PlaceAnimal(Player player, string animalType, Point location)
        {
            if (!IsValidPlacement(player, location))
            {
                Console.WriteLine($"Invalid placement at ({location.x}, {location.y})");
                return false;
            }

            int maxPieces = numCellsX <= 5 ? 4 : 7;
            if (player.Pieces.Count(p => !(p is CatKing) && !(p is MouseKing)) >= maxPieces - 1)
            {
                Console.WriteLine($"Player {player.Id} has reached the maximum number of pieces");
                return false;
            }

            Animal animal = CreateAnimal(animalType, player);
            Zone zone = animalZones[location.y][location.x];
            zone.occupant = animal;
            animal.location = location;
            player.AddPiece(animal);

            Console.WriteLine($"Player {player.Id} placed {animalType} at ({location.x}, {location.y})");
            GameStateChanged?.Invoke();
            return true;
        }

        public static bool IsValidPlacement(Player player, Point location)
        {
            int startColumn = player.Id == 1 ? 0 : (riverColumn + 1);
            int endColumn = player.Id == 1 ? (riverColumn - 1) : (numCellsX - 1);

            Console.WriteLine($"Checking location ({location.x}, {location.y}) for player {player.Id}");
            Console.WriteLine($"Valid range: {startColumn} to {endColumn}");

            if (location.x < startColumn || location.x > endColumn)
            {
                Console.WriteLine($"Location ({location.x}, {location.y}) is out of valid range for player {player.Id}");
                return false;
            }

            if (animalZones[location.y][location.x].occupant != null)
            {
                Console.WriteLine($"Location ({location.x}, {location.y}) is already occupied");
                return false;
            }

            Console.WriteLine($"Location ({location.x}, {location.y}) is a valid placement");
            return true;
        }

        private static Animal CreateAnimal(string animalType, Player owner)
        {
            Animal.Faction faction = owner.Id == 1 ? Animal.Faction.Cat : Animal.Faction.Mouse;

            switch (animalType)
            {
                case "Leopard": return new Leopard($"Leopard_{owner.Id}", faction);
                case "Tiger": return new Tiger($"Tiger_{owner.Id}", faction);
                case "Mouse": return new Mouse($"Mouse_{owner.Id}", faction);
                case "Squirrel": return new Squirrel($"Squirrel_{owner.Id}", faction);
                case "CatKing": return new CatKing($"CatKing_{owner.Id}");
                case "MouseKing": return new MouseKing($"MouseKing_{owner.Id}");
                default: throw new ArgumentException($"Unknown animal type: {animalType}");
            }
        }

        private static Zone GetRandomEmptyZone(int startRow, int endRow, int startColumn, int endColumn)
        {
            Random random = new Random();
            List<Zone> emptyZones = animalZones
                .Skip(startRow).Take(endRow - startRow + 1)
                .SelectMany(row => row.Skip(startColumn).Take(endColumn - startColumn + 1))
                .Where(zone => zone.occupant == null && !zone.isRiver)
                .ToList();

            if (emptyZones.Count == 0)
            {
                throw new InvalidOperationException("No available empty zones");
            }

            return emptyZones[random.Next(emptyZones.Count)];
        }

        public static bool MoveAnimal(Animal animal, Point destination)
        {
            if (!IsValidMove(animal, destination))
            {
                return false;
            }

            bool moveSuccess = animal.Move(destination);

            if (moveSuccess)
            {
                ApplyInteractionEffects(animal);
            }

            UpdateGameState();
            return moveSuccess;
        }

        public static bool IsValidMove(Animal animal, Point destination)
        {
            if (!animal.CanMoveAndAttackThisTurn)
            {
                return false;
            }

            int distance = Math.Abs(animal.location.x - destination.x) + Math.Abs(animal.location.y - destination.y);
            if (distance > animal.MaxMoveDistance || distance == 0)
            {
                return false;
            }

            if (!IsWithinBoard(destination))
            {
                return false;
            }

            Zone destinationZone = animalZones[destination.y][destination.x];
            if (destinationZone.isRiver && !(animal is Leopard && ((Leopard)animal).CanCrossRiverFreely))
            {
                return false;
            }

            if (destinationZone.occupant != null && destinationZone.occupant.Owner == animal.Owner)
            {
                return false;
            }

            if (animal.location.x != destination.x && animal.location.y != destination.y)
            {
                return false;
            }

            return true;
        }

        private static bool IsWithinBoard(Point point)
        {
            return point.x >= 0 && point.x < numCellsX && point.y >= 0 && point.y < numCellsY;
        }

        public static bool IsCrossingRiver(Point start, Point end)
        {
            return (start.x <= riverColumn && end.x > riverColumn) || (start.x > riverColumn && end.x <= riverColumn);
        }

        public static List<Point> GetValidMoves(Animal animal)
        {
            List<Point> validMoves = new List<Point>();
            int moveDistance = animal.GetModifiedMoveDistance();

            for (int dx = -moveDistance; dx <= moveDistance; dx++)
            {
                for (int dy = -moveDistance; dy <= moveDistance; dy++)
                {
                    if (Math.Abs(dx) + Math.Abs(dy) > moveDistance) continue;
                    if (dx != 0 && dy != 0) continue; // Only allow straight-line moves

                    Point destination = new Point { x = animal.location.x + dx, y = animal.location.y + dy };
                    if (IsValidMove(animal, destination))
                    {
                        validMoves.Add(destination);
                    }
                }
            }
            return validMoves;
        }

        public static void DebugPrintValidMoves(Animal animal)
        {
            var moves = GetValidMoves(animal);
            Console.WriteLine($"Valid moves for {animal.name} at ({animal.location.x}, {animal.location.y}):");
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
                    HandlePlacementClick(clickedZone);
                    break;
                case GamePhase.Playing:
                    HandlePlayingClick(clickedZone);
                    break;
                case GamePhase.SquirrelAbility:
                    HandleSquirrelAbilityClick(clickedZone);
                    break;
                // Possibly other phases...
                default:
                    Console.WriteLine($"Unhandled game phase: {CurrentPhase}");
                    break;
            }

            GameStateChanged?.Invoke();
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
                    selectedAnimal = animal;
                }
            }
            else
            {
                if (clickedZone.occupant == selectedAnimal)
                {
                    selectedAnimal = null;
                }
                else if (MoveOrAttack(selectedAnimal, clickedZone.location))
                {
                    EndTurn();
                }
                selectedAnimal = null;
            }

            if (CheckVictory())
            {
                CurrentPhase = GamePhase.GameOver;
            }
        }

        private static void HandleSquirrelAbilityClick(Zone clickedZone)
        {
            if (clickedZone.occupant is Animal target && target.Owner != CurrentPlayer)
            {
                AddSquirrelTarget(target);
            }
        }

        private static void EndTurn()
        {
            // Update effects for all animals
            foreach (var row in animalZones)
            {
                foreach (var zone in row)
                {
                    if (zone.occupant is Animal animal)
                    {
                        animal.UpdateEffects();
                    }
                }
            }

            // Switch current player
            CurrentPlayer = (CurrentPlayer == Player1) ? Player2 : Player1;

            // Reset move and attack status for all pieces of the current player
            foreach (var piece in CurrentPlayer.Pieces)
            {
                piece.CanMoveAndAttackThisTurn = true;
            }

            // Add any logic that needs to be executed at the start of the turn here

            // Trigger the GameStateChanged event to notify the UI to update
            GameStateChanged?.Invoke();
        }

        public static Animal GetSelectedAnimal()
        {
            return selectedAnimal;
        }

        public static bool IsAnimalSelected(Point location)
        {
            return selectedAnimal != null &&
                   selectedAnimal.location.x == location.x &&
                   selectedAnimal.location.y == location.y;
        }

        public static void ApplyInteractionEffects(Animal movedAnimal)
        {
            movedAnimal.DetectNearbyAnimals();
            movedAnimal.HandleInteractions();

            // Detect and handle interactions for nearby animals as well
            foreach (var nearbyAnimal in movedAnimal.NearbyAnimals)
            {
                nearbyAnimal.DetectNearbyAnimals();
                nearbyAnimal.HandleInteractions();
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
                    target.ApplyEffect(effect, 1, source);
                    target.MoveDistanceModifier = 1;
                    Console.WriteLine($"{source.name} attracted {target.name}");
                    break;
                case InteractionEffect.Repel:
                    target.ApplyEffect(effect, 1, source);
                    target.MoveDistanceModifier = -1;
                    Console.WriteLine($"{source.name} repelled {target.name}");
                    break;
                case InteractionEffect.Pursue:
                    source.CanAttackAgain = true;
                    Console.WriteLine($"{source.name} can attack {target.name} again");
                    break;
                case InteractionEffect.Flee:
                    target.ApplyEffect(effect, 0, source);
                    target.IsFleeingFrom = source;
                    Console.WriteLine($"{target.name} is fleeing from {source.name}");
                    break;
                case InteractionEffect.Intimidate:
                    if (new Random().Next(2) == 0) // 50% chance
                    {
                        target.ApplyEffect(InteractionEffect.Paralyze, 1, source);
                        target.IsParalyzed = true;
                        Console.WriteLine($"{source.name} intimidated and paralyzed {target.name}");
                    }
                    else
                    {
                        Console.WriteLine($"{source.name} attempted to intimidate {target.name}, but failed");
                    }
                    break;
                case InteractionEffect.Paralyze:
                    target.ApplyEffect(effect, 1, source);
                    target.IsParalyzed = true;
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
                    return source is CatKing || source is MouseKing ? 2 : 1;
                case InteractionEffect.Pursue:
                    return source is Leopard ? 2 : 1;
                case InteractionEffect.Flee:
                    return target is Mouse ? 2 : 1;
                case InteractionEffect.Intimidate:
                    return 1;
                default:
                    return 0;
            }
        }

        // Modified: GetInteractionEffect method
        public static InteractionEffect GetInteractionEffect(Animal source, Animal target)
        {
            if (source is Leopard || source is Tiger)
            {
                if (target is Mouse || target is Squirrel)
                    return InteractionEffect.Pursue;
                if (source.GetType() == target.GetType())
                    return InteractionEffect.Repel;
            }
            else if (source is Mouse || source is Squirrel)
            {
                if (target is Leopard || target is Tiger)
                    return InteractionEffect.Flee;
                if (source.GetType() == target.GetType())
                    return InteractionEffect.Attract;
            }
            else if (source is CatKing)
            {
                if (target is Leopard || target is Tiger)
                    return InteractionEffect.Attract;
                if (target is Mouse || target is Squirrel)
                    return InteractionEffect.Intimidate;
            }
            else if (source is MouseKing)
            {
                if (target is Leopard || target is Tiger)
                    return InteractionEffect.Repel;
                if (target is Mouse || target is Squirrel)
                    return InteractionEffect.Attract;
            }
            return InteractionEffect.None;
        }

        public static bool IsSeparatedByRiver(Animal animal1, Animal animal2)
        {
            return (animal1.location.x <= riverColumn && animal2.location.x > riverColumn) ||
                   (animal1.location.x > riverColumn && animal2.location.x <= riverColumn);
        }

        private static bool CheckVictory()
        {
            // Check if either player's king is defeated or all pieces are eliminated
            return (Player1.King == null || Player1.Pieces.Count == 0) ||
                   (Player2.King == null || Player2.Pieces.Count == 0);
        }

        public static bool UseSpecialAbility(Animal animal)
        {
            if (animal.UseSpecialAbility())
            {
                animal.CanMoveAndAttackThisTurn = !animal.ShouldEndTurnAfterSpecialAbility();

                bool abilitySuccess = false;

                if (animal is Tiger tiger)
                {
                    abilitySuccess = StartTigerAbility(tiger);
                }
                else if (animal is Squirrel squirrel)
                {
                    StartSquirrelAbility(squirrel);
                    abilitySuccess = true;
                }
                else if (animal is CatKing catKing)
                {
                    abilitySuccess = CatKingRally(catKing);
                }
                else if (animal is MouseKing mouseKing)
                {
                    abilitySuccess = MouseKingRevive(mouseKing);
                }
                else if (animal is Leopard)
                {
                    abilitySuccess = true; // Leopard's ability always succeeds
                }

                if (abilitySuccess && animal.ShouldEndTurnAfterSpecialAbility())
                {
                    EndTurn();
                }

                GameStateChanged?.Invoke();
                return abilitySuccess;
            }
            return false;
        }

        public static void SkipTurn()
        {
            EndTurn();
            GameStateChanged?.Invoke();
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

            GameStateChanged?.Invoke();
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
                   defender.Owner.ExchangeCount == 0;
        }

        public static void RemoveAnimal(Animal animal)
        {
            Zone zone = animalZones[animal.location.y][animal.location.x];
            zone.occupant = null;
            animal.Owner.RemovePiece(animal);
        }

        // New: Trigger exchange effect
        public static void TriggerExchangeEffect(Animal attacker, Animal defender)
        {
            if (CanExchangePieces(attacker, defender))
            {
                ExchangePieces(attacker, defender);
            }
        }

        // New: Reset game
        public static void ResetGame()
        {
            CurrentPhase = GamePhase.Setup;
            animalZones.Clear();
            Player1 = null;
            Player2 = null;
            CurrentPlayer = null;
            selectedAnimal = null;
            GameStateChanged?.Invoke();
        }

        public static bool MoveOrAttack(Animal animal, Point destination)
        {
            if (!IsValidMove(animal, destination))
            {
                return false;
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
                ApplyInteractionEffects(animal);
            }

            return moveSuccess;
        }

        public static void ProcessAttack(Animal attacker, Animal defender)
        {
            attacker.Attack(defender);

            // Handle global effects and complex logic
            if (defender.CurrentEffect == InteractionEffect.Flee)
            {
                int fleeDistance = new Random().Next(1, 3); // 1-2 squares
                Point fleeDestination = GetFleeDestination(defender, fleeDistance);
                if (fleeDestination.x != -1 && fleeDestination.y != -1)
                {
                    MoveAnimal(defender, fleeDestination);
                }
            }

            if (defender.healthPoints <= 0)
            {
                RemoveAnimal(defender);
            }

            // Apply interaction effects
            ApplyInteractionEffects(attacker);
            ApplyInteractionEffects(defender);

            // Check if the exchange effect is triggered
            if (CanExchangePieces(attacker, defender))
            {
                TriggerExchangeEffect(attacker, defender);
            }

            GameStateChanged?.Invoke();
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
                        animal.UpdateEffects();
                    }
                }
            }

            // Check victory conditions
            if (CheckVictory())
            {
                CurrentPhase = GamePhase.GameOver;
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
                EndTurn();
            }

            // Trigger the GameStateChanged event to notify the UI to update
            GameStateChanged?.Invoke();
        }

        private static Point GetFleeDestination(Animal defender, int fleeDistance)
        {
            List<Point> possibleDestinations = new List<Point>();
            int[] dx = { -1, 1, 0, 0 };  // Left, Right, Up, Down
            int[] dy = { 0, 0, -1, 1 };

            // Check all possible flee directions
            for (int i = 0; i < 4; i++)
            {
                int newX = defender.location.x + dx[i] * fleeDistance;
                int newY = defender.location.y + dy[i] * fleeDistance;

                // Check if the new position is within the board and empty
                if (IsValidFleeDestination(new Point { x = newX, y = newY }, defender))
                {
                    possibleDestinations.Add(new Point { x = newX, y = newY });
                }
            }

            // If no valid flee positions, try a shorter distance
            if (possibleDestinations.Count == 0 && fleeDistance > 1)
            {
                return GetFleeDestination(defender, fleeDistance - 1);
            }

            // If multiple possible positions, choose the one farthest from enemies
            if (possibleDestinations.Count > 0)
            {
                return ChooseBestFleeDestination(defender, possibleDestinations);
            }

            // If unable to flee, return the original position
            return defender.location;
        }

        private static bool IsValidFleeDestination(Point destination, Animal defender)
        {
            // Check if within the board
            if (!IsWithinBoard(destination))
            {
                return false;
            }

            // Check if the destination is empty
            if (animalZones[destination.y][destination.x].occupant != null)
            {
                return false;
            }

            // Check if crossing the river (unless it's an animal that can cross)
            if (IsCrossingRiver(defender.location, destination) && !(defender is Leopard && ((Leopard)defender).CanCrossRiverFreely))
            {
                return false;
            }

            return true;
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
                enemies.Min(enemy => Math.Abs(dest.x - enemy.location.x) + Math.Abs(dest.y - enemy.location.y))
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
            MoveAnimal(selectedPiece, destination);

            // Restore health points
            catKing.healthPoints = Math.Min(catKing.healthPoints + 2, 30);  // Assuming maximum health is 30

            Console.WriteLine($"{catKing.name} used the rally ability, moving {selectedPiece.name} and restoring 2 health points");
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
            // Modified: Added null check and type conversion
            PlaceAnimal(CurrentPlayer, revivedPiece.species, emptyAdjacentSpot.Value);
            CurrentPlayer.DeadPieces.Remove(revivedPiece);

            Console.WriteLine($"{mouseKing.name} used the revive ability, reviving {revivedPiece.name}");
            return true;
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

            return null;
        }

        public static void StartSquirrelAbility(Squirrel squirrel)
        {
            currentSquirrel = squirrel;
            squirrelTargets.Clear();
            CurrentPhase = GamePhase.SquirrelAbility;
            Console.WriteLine("Please select two targets to attack.");
            GameStateChanged?.Invoke();
        }

        public static void AddSquirrelTarget(Animal target)
        {
            if (squirrelTargets.Count < 2 && !squirrelTargets.Contains(target))
            {
                squirrelTargets.Add(target);
                Console.WriteLine($"Selected target: {target.name}");
                if (squirrelTargets.Count == 2)
                {
                    ExecuteSquirrelAbility();
                }
            }
        }

        private static void ApplyDirectDamage(Animal source, Animal target, int damage)
        {
            target.TakeDamage(damage);
            Console.WriteLine($"{source.name} dealt {damage} direct damage to {target.name}!");

            if (target.healthPoints <= 0)
            {
                Console.WriteLine($"{target.name} has been defeated!");
                RemoveAnimal(target);
            }
        }

        private static void ExecuteSquirrelAbility()
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
                    ApplyDirectDamage(currentSquirrel, target, currentSquirrel.attackPower);
                }
            }

            // Reset variables related to squirrel ability
            currentSquirrel = null;
            squirrelTargets.Clear();

            // End the current turn
            EndTurn();

            CurrentPhase = GamePhase.Playing;
            GameStateChanged?.Invoke();
        }

        public static bool StartTigerAbility(Tiger tiger)
        {
            Console.WriteLine($"{tiger.name} used the roar ability!");

            // Get the enemy king
            Animal enemyKing = (tiger.Owner == Player1) ? Player2.King : Player1.King;

            if (enemyKing != null)
            {
                ApplyDirectDamage(tiger, enemyKing, tiger.attackPower);
                Console.WriteLine($"{tiger.name} dealt {tiger.attackPower} damage to {enemyKing.name}!");
                EndTurn();
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
                    return new Point { x = location.x, y = location.y - 1 };
                case Direction.Down:
                    return new Point { x = location.x, y = location.y + 1 };
                case Direction.Left:
                    return new Point { x = location.x - 1, y = location.y };
                case Direction.Right:
                    return new Point { x = location.x + 1, y = location.y };
                default:
                    throw new ArgumentException("Invalid direction");
            }
        }

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
    }
}
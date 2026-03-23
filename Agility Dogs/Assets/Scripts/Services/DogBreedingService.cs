using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;

namespace AgilityDogs.Services
{
    /// <summary>
    /// DogBreedingService - Handles puppy creation and trait inheritance
    /// Used in Career mode to breed and select puppies for training
    /// </summary>
    public class DogBreedingService : MonoBehaviour
    {
        public static DogBreedingService Instance { get; private set; }

        [Header("Available Breeds")]
        [SerializeField] private BreedData[] availableBreeds;

        [Header("Trait Configuration")]
        [SerializeField] private int traitsPerPuppy = 2;
        [SerializeField] private float mutationChance = 0.1f;

        // Breeding state
        private List<PuppyData> availablePuppies = new List<PuppyData>();
        private PuppyData selectedPuppy;
        private List<BreedData> parentBreeds = new List<BreedData>();

        // Events
        public event Action<PuppyData> OnPuppyGenerated;
        public event Action<PuppyData> OnPuppySelected;
        public event Action<PuppyData> OnPuppyTrained;

        // Properties
        public List<PuppyData> AvailablePuppies => availablePuppies;
        public PuppyData SelectedPuppy => selectedPuppy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAvailableBreeds();
        }

        #region Breed Loading

        /// <summary>
        /// Load all available breed data from resources
        /// </summary>
        private void LoadAvailableBreeds()
        {
            if (availableBreeds == null || availableBreeds.Length == 0)
            {
                availableBreeds = Resources.LoadAll<BreedData>("Data/Breeds");
                Debug.Log($"[DogBreedingService] Loaded {availableBreeds.Length} breeds");
            }
        }

        /// <summary>
        /// Get all available breeds
        /// </summary>
        public BreedData[] GetAllBreeds()
        {
            LoadAvailableBreeds();
            return availableBreeds;
        }

        /// <summary>
        /// Get a breed by name
        /// </summary>
        public BreedData GetBreedByName(string breedName)
        {
            return availableBreeds.FirstOrDefault(b => b.breedName == breedName);
        }

        #endregion

        #region Puppy Generation

        /// <summary>
        /// Generate a new puppy with random traits
        /// Called when starting a new career or breeding
        /// </summary>
        public PuppyData GeneratePuppy(BreedData breed = null, string puppyName = null)
        {
            // If no breed specified, pick a random starter breed
            if (breed == null)
            {
                breed = GetRandomStarterBreed();
            }

            // Generate random traits
            List<PuppyTrait> traits = GenerateRandomTraits(traitsPerPuppy);

            // Create puppy data
            PuppyData puppy = new PuppyData
            {
                puppyId = Guid.NewGuid().ToString(),
                puppyName = puppyName ?? GenerateRandomPuppyName(breed),
                breedData = breed,
                birthDate = DateTime.Now,
                traits = traits,
                generation = 1,
                baseStats = GenerateBaseStats(breed, traits),
                trainingProgress = new Dictionary<TrainingSkill, int>(),
                competitionsEntered = 0,
                competitionsWon = 0,
                isChampion = false
            };

            // Initialize training progress
            foreach (TrainingSkill skill in Enum.GetValues(typeof(TrainingSkill)))
            {
                puppy.trainingProgress[skill] = 0;
            }

            availablePuppies.Add(puppy);
            OnPuppyGenerated?.Invoke(puppy);

            Debug.Log($"[DogBreedingService] Generated puppy: {puppy.puppyName} ({breed.displayName}) with traits: {string.Join(", ", traits)}");

            return puppy;
        }

        /// <summary>
        /// Breed two parent dogs to create a puppy
        /// </summary>
        public PuppyData BreedPuppies(PuppyData parent1, PuppyData parent2, string puppyName = null)
        {
            if (parent1 == null || parent2 == null)
            {
                Debug.LogError("[DogBreedingService] Cannot breed null parents");
                return null;
            }

            // Inherit breed from dominant parent (or random)
            BreedData childBreed = UnityEngine.Random.value > 0.5f ? parent1.breedData : parent2.breedData;

            // Inherit traits from both parents
            List<PuppyTrait> inheritedTraits = InheritTraits(parent1.traits, parent2.traits);

            // Chance of mutation
            if (UnityEngine.Random.value < mutationChance)
            {
                PuppyTrait randomTrait = GetRandomTrait();
                if (!inheritedTraits.Contains(randomTrait))
                {
                    if (inheritedTraits.Count >= traitsPerPuppy)
                    {
                        inheritedTraits[UnityEngine.Random.Range(0, inheritedTraits.Count)] = randomTrait;
                    }
                    else
                    {
                        inheritedTraits.Add(randomTrait);
                    }
                    Debug.Log($"[DogBreedingService] Trait mutation occurred: {randomTrait}");
                }
            }

            // Create puppy with inherited traits
            PuppyData puppy = new PuppyData
            {
                puppyId = Guid.NewGuid().ToString(),
                puppyName = puppyName ?? GenerateRandomPuppyName(childBreed),
                breedData = childBreed,
                birthDate = DateTime.Now,
                traits = inheritedTraits,
                generation = Mathf.Max(parent1.generation, parent2.generation) + 1,
                baseStats = GenerateBaseStats(childBreed, inheritedTraits),
                trainingProgress = new Dictionary<TrainingSkill, int>(),
                competitionsEntered = 0,
                competitionsWon = 0,
                isChampion = false,
                parent1Id = parent1.puppyId,
                parent2Id = parent2.puppyId
            };

            // Initialize training progress
            foreach (TrainingSkill skill in Enum.GetValues(typeof(TrainingSkill)))
            {
                puppy.trainingProgress[skill] = 0;
            }

            availablePuppies.Add(puppy);
            OnPuppyGenerated?.Invoke(puppy);

            Debug.Log($"[DogBreedingService] Bred puppy: {puppy.puppyName} (Gen {puppy.generation}) from {parent1.puppyName} x {parent2.puppyName}");

            return puppy;
        }

        /// <summary>
        /// Generate multiple puppies to choose from
        /// </summary>
        public List<PuppyData> GeneratePuppyLitter(int count, BreedData breed = null)
        {
            List<PuppyData> litter = new List<PuppyData>();

            for (int i = 0; i < count; i++)
            {
                PuppyData puppy = GeneratePuppy(breed);
                litter.Add(puppy);
            }

            return litter;
        }

        #endregion

        #region Puppy Selection

        /// <summary>
        /// Select a puppy for training
        /// </summary>
        public void SelectPuppy(PuppyData puppy)
        {
            if (puppy == null) return;

            selectedPuppy = puppy;
            OnPuppySelected?.Invoke(puppy);

            Debug.Log($"[DogBreedingService] Selected puppy: {puppy.puppyName}");

            // Save selection
            SavePuppySelection(puppy);
        }

        /// <summary>
        /// Get the currently selected puppy
        /// </summary>
        public PuppyData GetSelectedPuppy()
        {
            if (selectedPuppy == null)
            {
                LoadPuppySelection();
            }
            return selectedPuppy;
        }

        #endregion

        #region Trait Generation

        /// <summary>
        /// Generate random traits for a puppy
        /// </summary>
        private List<PuppyTrait> GenerateRandomTraits(int count)
        {
            List<PuppyTrait> traits = new List<PuppyTrait>();
            List<PuppyTrait> allTraits = Enum.GetValues(typeof(PuppyTrait)).Cast<PuppyTrait>().ToList();

            // Shuffle and take first N
            allTraits.Shuffle();

            for (int i = 0; i < Mathf.Min(count, allTraits.Count); i++)
            {
                traits.Add(allTraits[i]);
            }

            return traits;
        }

        /// <summary>
        /// Inherit traits from both parents
        /// </summary>
        private List<PuppyTrait> InheritTraits(List<PuppyTrait> parent1Traits, List<PuppyTrait> parent2Traits)
        {
            List<PuppyTrait> inherited = new List<PuppyTrait>();

            // Take one trait from each parent
            if (parent1Traits.Count > 0)
            {
                inherited.Add(parent1Traits[UnityEngine.Random.Range(0, parent1Traits.Count)]);
            }
            if (parent2Traits.Count > 0)
            {
                PuppyTrait trait = parent2Traits[UnityEngine.Random.Range(0, parent2Traits.Count)];
                if (!inherited.Contains(trait))
                {
                    inherited.Add(trait);
                }
            }

            // Fill remaining slots with random traits if needed
            while (inherited.Count < traitsPerPuppy)
            {
                PuppyTrait randomTrait = GetRandomTrait();
                if (!inherited.Contains(randomTrait))
                {
                    inherited.Add(randomTrait);
                }
            }

            return inherited.Take(traitsPerPuppy).ToList();
        }

        /// <summary>
        /// Get a random trait
        /// </summary>
        private PuppyTrait GetRandomTrait()
        {
            Array values = Enum.GetValues(typeof(PuppyTrait));
            return (PuppyTrait)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        #endregion

        #region Stat Generation

        /// <summary>
        /// Generate base stats for a puppy based on breed and traits
        /// </summary>
        private PuppyStats GenerateBaseStats(BreedData breed, List<PuppyTrait> traits)
        {
            PuppyStats stats = new PuppyStats
            {
                // Start with breed base stats (slightly reduced for puppy)
                speed = breed.maxSpeed * 0.6f,
                acceleration = breed.acceleration * 0.6f,
                agility = breed.turnRate / 540f, // Normalize to 0-1
                jumpPower = breed.jumpPower * 0.6f,
                stamina = 0.5f,
                intelligence = 0.5f,
                focus = 0.5f,
                confidence = 0.5f
            };

            // Apply trait modifiers
            foreach (PuppyTrait trait in traits)
            {
                ApplyTraitModifier(ref stats, trait);
            }

            return stats;
        }

        /// <summary>
        /// Apply trait modifiers to stats
        /// </summary>
        private void ApplyTraitModifier(ref PuppyStats stats, PuppyTrait trait)
        {
            switch (trait)
            {
                case PuppyTrait.Energetic:
                    stats.speed *= 1.1f;
                    stats.stamina *= 1.2f;
                    stats.focus *= 0.9f;
                    break;

                case PuppyTrait.Calm:
                    stats.stamina *= 1.1f;
                    stats.focus *= 1.2f;
                    stats.speed *= 0.9f;
                    break;

                case PuppyTrait.Intelligent:
                    stats.intelligence *= 1.3f;
                    break;

                case PuppyTrait.Stubborn:
                    stats.intelligence *= 0.8f;
                    stats.confidence *= 1.1f;
                    break;

                case PuppyTrait.Agile:
                    stats.agility *= 1.2f;
                    break;

                case PuppyTrait.Strong:
                    stats.jumpPower *= 1.2f;
                    stats.speed *= 1.05f;
                    break;

                case PuppyTrait.Sensitive:
                    stats.intelligence *= 1.1f;
                    stats.focus *= 1.1f;
                    stats.confidence *= 0.9f;
                    break;

                case PuppyTrait.Distracted:
                    stats.focus *= 0.7f;
                    stats.speed *= 1.1f;
                    break;

                case PuppyTrait.Confident:
                    stats.confidence *= 1.3f;
                    break;

                case PuppyTrait.Nervous:
                    stats.confidence *= 0.7f;
                    stats.focus *= 0.8f;
                    break;
            }
        }

        #endregion

        #region Breed Selection

        /// <summary>
        /// Get a random starter breed (simpler breeds for beginners)
        /// </summary>
        private BreedData GetRandomStarterBreed()
        {
            // Starter breeds: Border Collie, Labrador, Golden Retriever
            string[] starterBreeds = { "Border_Collie", "Labrador", "GoldenRetriever", "Corgi" };

            foreach (string breedName in starterBreeds)
            {
                BreedData breed = GetBreedByName(breedName);
                if (breed != null) return breed;
            }

            // Fallback to any available breed
            return availableBreeds.Length > 0 ? availableBreeds[0] : null;
        }

        /// <summary>
        /// Get all unlocked breeds for a career level
        /// </summary>
        public BreedData[] GetUnlockedBreeds(int careerLevel)
        {
            List<BreedData> unlocked = new List<BreedData>();

            // Starter breeds available from level 1
            string[] starterBreeds = { "Border_Collie", "Labrador", "GoldenRetriever", "Corgi" };
            foreach (string breedName in starterBreeds)
            {
                BreedData breed = GetBreedByName(breedName);
                if (breed != null) unlocked.Add(breed);
            }

            // Unlock more breeds as level increases
            if (careerLevel >= 5)
            {
                string[] midBreeds = { "Shepherd", "JackRussellTerrier", "ShibaInu", "Beagle" };
                foreach (string breedName in midBreeds)
                {
                    BreedData breed = GetBreedByName(breedName);
                    if (breed != null) unlocked.Add(breed);
                }
            }

            if (careerLevel >= 10)
            {
                string[] advancedBreeds = { "Poodle", "Dalmatian", "Husky", "Doberman" };
                foreach (string breedName in advancedBreeds)
                {
                    BreedData breed = GetBreedByName(breedName);
                    if (breed != null) unlocked.Add(breed);
                }
            }

            return unlocked.ToArray();
        }

        #endregion

        #region Puppy Names

        /// <summary>
        /// Generate a random puppy name based on breed
        /// </summary>
        private string GenerateRandomPuppyName(BreedData breed)
        {
            string[] prefixes = { "Speed", "Dash", "Bolt", "Flash", "Swift", "Quick", "Zippy", "Zoom", "Blaze", "Rocket" };
            string[] suffixes = { "ster", "y", "ie", "o", "ster", "kin", "let", "ette" };

            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];

            return prefix + suffix;
        }

        /// <summary>
        /// Set a custom name for a puppy
        /// </summary>
        public void SetPuppyName(PuppyData puppy, string newName)
        {
            if (puppy != null && !string.IsNullOrEmpty(newName))
            {
                puppy.puppyName = newName;
                Debug.Log($"[DogBreedingService] Renamed puppy to: {newName}");
            }
        }

        #endregion

        #region Persistence

        private void SavePuppySelection(PuppyData puppy)
        {
            if (puppy == null) return;

            PlayerPrefs.SetString("SelectedPuppyId", puppy.puppyId);
            PlayerPrefs.SetString("SelectedPuppyName", puppy.puppyName);
            PlayerPrefs.SetString("SelectedPuppyBreed", puppy.breedData.breedName);
            PlayerPrefs.Save();
        }

        private void LoadPuppySelection()
        {
            string puppyId = PlayerPrefs.GetString("SelectedPuppyId", "");
            if (!string.IsNullOrEmpty(puppyId))
            {
                selectedPuppy = availablePuppies.FirstOrDefault(p => p.puppyId == puppyId);
            }
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Puppy data structure for career mode
    /// </summary>
    [Serializable]
    public class PuppyData
    {
        public string puppyId;
        public string puppyName;
        public BreedData breedData;
        public DateTime birthDate;
        public List<PuppyTrait> traits;
        public int generation;
        public PuppyStats baseStats;
        public Dictionary<TrainingSkill, int> trainingProgress;
        public int competitionsEntered;
        public int competitionsWon;
        public bool isChampion;
        public string parent1Id;
        public string parent2Id;

        // Calculated stats (base + training)
        public float TotalTrainingLevel => trainingProgress?.Values.Sum() ?? 0;
        public float TrainingProgressPercent => TotalTrainingLevel / (trainingProgress?.Count * 100f ?? 1f);
    }

    /// <summary>
    /// Puppy stats affected by breed and traits
    /// </summary>
    [Serializable]
    public class PuppyStats
    {
        public float speed;
        public float acceleration;
        public float agility;
        public float jumpPower;
        public float stamina;
        public float intelligence;
        public float focus;
        public float confidence;

        /// <summary>
        /// Get overall stat average
        /// </summary>
        public float GetOverallRating()
        {
            return (speed + acceleration + agility + jumpPower + stamina + intelligence + focus + confidence) / 8f;
        }
    }

    #endregion

    #region Extensions

    /// <summary>
    /// List extension methods
    /// </summary>
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    #endregion
}

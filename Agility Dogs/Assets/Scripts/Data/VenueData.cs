using UnityEngine;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "NewVenueData", menuName = "Agility Dogs/Venue Data")]
    public class VenueData : ScriptableObject
    {
        [Header("Venue Info")]
        public string venueId;
        public string venueName;
        public string displayName;
        [TextArea(3, 6)]
        public string description;

        [Header("Location")]
        public string city;
        public string state;
        public string country;

        [Header("Visual")]
        public GameObject venuePrefab;
        public Sprite thumbnail;
        public Material skyboxMaterial;
        public Color ambientColor = Color.white;

        [Header("Environment")]
        public AudioClip ambientSound;
        public WeatherType defaultWeather = WeatherType.Clear;

        [Header("Course Configuration")]
        public int courseCapacity = 20;
        public bool supportsStandard = true;
        public bool supportsJumpersWithWeaves = true;
        public bool supportsChampionship = false;

        [Header("Difficulty")]
        [Range(1, 10)]
        public int difficultyRating = 5;

        [Header("Prestige")]
        [Range(1, 10)]
        public int prestigeLevel = 1;
        public int requiredLevelToUnlock = 1;
        public bool isChampionshipVenue = false;

        [Header("Unlock")]
        public bool isUnlockedByDefault = true;
        public int unlockCost = 1000;

        [Header("Events")]
        public string[] hostedEvents = new string[] { "Local Agility Trial" };

        [Header("Amenities")]
        public bool hasIndoorArena = false;
        public bool hasAgilityEquipment = true;
        public bool hasWarmUpArea = true;
        public bool hasSpectatorSeating = true;

        [Header("Flavor")]
        [TextArea(2, 4)]
        public string venueHistory;
        public string[] specialFeatures = new string[0];
    }

    public enum WeatherType
    {
        Clear,
        Cloudy,
        LightRain,
        Overcast,
        Indoor
    }
}

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using AgilityDogs.Data;
using AgilityDogs.Services;
using System.Collections.Generic;

namespace AgilityDogs.Editor
{
    /// <summary>
    /// Editor utility to populate BestInShowDialogue with Arthur and Buck's lines
    /// </summary>
    public static class BestInShowDialoguePopulator
    {
        [MenuItem("Agility Dogs/Populate Best In Show Dialogue")]
        public static void PopulateDialogue()
        {
            // Find or create the BestInShowDialogue asset
            string assetPath = "Assets/Data/BestInShowDialogue.asset";
            BestInShowDialogue dialogue = AssetDatabase.LoadAssetAtPath<BestInShowDialogue>(assetPath);
            
            if (dialogue == null)
            {
                // Create new asset
                dialogue = ScriptableObject.CreateInstance<BestInShowDialogue>();
                
                // Ensure directory exists
                string dir = System.IO.Path.GetDirectoryName(assetPath);
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                    AssetDatabase.Refresh();
                }
                
                AssetDatabase.CreateAsset(dialogue, assetPath);
                Debug.Log($"[BestInShow] Created new dialogue asset at {assetPath}");
            }
            
            // Populate all pools
            PopulateMatchIntroPool(dialogue);
            PopulateWeavePolesPool(dialogue);
            PopulateContactObstaclesPool(dialogue);
            PopulateTunnelPool(dialogue);
            PopulateTeeterTotterPool(dialogue);
            PopulateJumpsPool(dialogue);
            PopulateMistakesPool(dialogue);
            PopulateFinishLinePool(dialogue);
            
            // Initialize pools
            dialogue.InitializeAllPools();
            
            // Save
            EditorUtility.SetDirty(dialogue);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[BestInShow] Populated dialogue with {dialogue.GetTotalLineCount()} total lines!");
            EditorUtility.DisplayDialog("Best In Show", 
                $"Successfully populated dialogue with {dialogue.GetTotalLineCount()} lines!\n\n" +
                "Arthur (Play-by-Play): Technical, knowledgeable\n" +
                "Buck (Color): Ignorant, confident, hilarious", 
                "OK");
        }
        
        #region Pool Populators
        
        private static void PopulateMatchIntroPool(BestInShowDialogue dialogue)
        {
            dialogue.matchIntroPool = new CommentaryStatePool
            {
                state = CommentaryState.MatchIntro,
                arthurDelay = 0f,
                buckDelay = 1f,
                buckChance = 0.5f,
                arthurPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("Welcome to the Grand Final. I'm Arthur Pendelton, and joining me is Buck Hastings.", AnnouncerType.Main),
                        new DialogueLineEntry("The Standard Course Time today is set at a blistering 48 seconds.", AnnouncerType.Main),
                        new DialogueLineEntry("We are looking for clean runs today. A single dropped bar could cost them the championship.", AnnouncerType.Main),
                        new DialogueLineEntry("The handlers have walked the course, and we are ready to begin.", AnnouncerType.Main),
                        new DialogueLineEntry("It's a flowing course, but the trap near the tunnel entry will test their handling.", AnnouncerType.Main),
                        new DialogueLineEntry("You have to admire the pedigree of these competitors.", AnnouncerType.Main),
                        new DialogueLineEntry("I'm expecting some aggressive blind crosses today to shave off time.", AnnouncerType.Main),
                        new DialogueLineEntry("The judge is in position, the crowd is hushed...", AnnouncerType.Main),
                        new DialogueLineEntry("A lot of energy on the starting line. You can see the dog vibrating with anticipation.", AnnouncerType.Main),
                        new DialogueLineEntry("Buck, I implore you, please do not eat your hotdog directly into the microphone.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1.5f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("Arthur, how much do you think that dog owes in taxes?", AnnouncerType.Color),
                        new DialogueLineEntry("I'm looking at the handler, Arthur. She's got great calves. Think she's single?", AnnouncerType.Color),
                        new DialogueLineEntry("If I were a dog, Arthur, I'd just bite the judge and go home.", AnnouncerType.Color),
                        new DialogueLineEntry("Do you think these dogs know they aren't tiny horses? Or do they think we're giant dogs?", AnnouncerType.Color),
                        new DialogueLineEntry("I brought a bag of beef jerky. You think they can smell it from the booth? Watch this.", AnnouncerType.Color),
                        new DialogueLineEntry("I've been doing my stretches, Arthur. If a handler goes down, I'm ready to tag in.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f }
                }
            };
        }
        
        private static void PopulateWeavePolesPool(BestInShowDialogue dialogue)
        {
            dialogue.weavePolesPool = new CommentaryStatePool
            {
                state = CommentaryState.WeavePoles,
                arthurDelay = 0f,
                buckDelay = 0.5f,
                buckChance = 0.4f,
                arthurPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("Beautiful entry into the weaves.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight bobble on pole four, but they recover.", AnnouncerType.Main),
                        new DialogueLineEntry("Textbook footwork through the weaves.", AnnouncerType.Main),
                        new DialogueLineEntry("Maintains an incredible, rhythmic cadence there.", AnnouncerType.Main),
                        new DialogueLineEntry("You can't teach that kind of lateral flexibility.", AnnouncerType.Main),
                        new DialogueLineEntry("Nailed the entry! That's where you make or break your time.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler is pushing the pace, staying lateral to the poles.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight hesitation, but they find the gap.", AnnouncerType.Main),
                        new DialogueLineEntry("Popped out early! That's going to be a costly fault.", AnnouncerType.Main),
                        new DialogueLineEntry("Look at the independence on that dog—the handler is already running to the next obstacle.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1.5f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("He's weaving like me leaving the pub on a Tuesday!", AnnouncerType.Color),
                        new DialogueLineEntry("I tried that once after a few margaritas. Broke a lamp.", AnnouncerType.Color),
                        new DialogueLineEntry("You know, if you tape a broom to a dog's tail doing that, you'd never have to sweep.", AnnouncerType.Color),
                        new DialogueLineEntry("Why don't they just go in a straight line, Arthur? Seems highly inefficient.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog is vibrating. Have they been giving them espresso?", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f }
                }
            };
        }
        
        private static void PopulateContactObstaclesPool(BestInShowDialogue dialogue)
        {
            dialogue.contactObstaclesPool = new CommentaryStatePool
            {
                state = CommentaryState.ContactObstacles,
                arthurDelay = 0f,
                buckDelay = 0.5f,
                buckChance = 0.4f,
                arthurPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("Approaching the A-frame, looking for a strong climb.", AnnouncerType.Main),
                        new DialogueLineEntry("Excellent contact on the descent. Two paws in the yellow.", AnnouncerType.Main),
                        new DialogueLineEntry("A very safe two-on-two-off position.", AnnouncerType.Main),
                        new DialogueLineEntry("Running the dog walk beautifully.", AnnouncerType.Main),
                        new DialogueLineEntry("Leapt right over the contact zone! That's a five-point fault.", AnnouncerType.Main),
                        new DialogueLineEntry("The judge's hand goes up. Missed the contact.", AnnouncerType.Main),
                        new DialogueLineEntry("Incredible acceleration up the ramp.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler decelerates perfectly to ensure the dog hits the yellow zone.", AnnouncerType.Main),
                        new DialogueLineEntry("A bit cautious over the apex, bleeding a few tenths of a second.", AnnouncerType.Main),
                        new DialogueLineEntry("Absolutely flawless execution on the A-frame.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1.5f, 1.5f, 1f, 1f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("I climbed an A-frame once. Had to call the fire department to get me down.", AnnouncerType.Color),
                        new DialogueLineEntry("If he slips, does he get workers' comp?", AnnouncerType.Color),
                        new DialogueLineEntry("That ramp looks like the roof of my first apartment.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, what happens if the dog just decides to take a nap up there?", AnnouncerType.Color),
                        new DialogueLineEntry("I bet you could launch a pretty good firework off the top of that thing.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f }
                }
            };
        }
        
        private static void PopulateTunnelPool(BestInShowDialogue dialogue)
        {
            dialogue.tunnelPool = new CommentaryStatePool
            {
                state = CommentaryState.Tunnel,
                arthurDelay = 0f,
                buckDelay = 0.5f,
                buckChance = 0.4f,
                arthurPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("Sent into the curved tunnel.", AnnouncerType.Main),
                        new DialogueLineEntry("A blind entry into the pipe!", AnnouncerType.Main),
                        new DialogueLineEntry("The handler executes a brilliant front cross while the dog is out of sight.", AnnouncerType.Main),
                        new DialogueLineEntry("Lost some traction exiting the tunnel on the turf.", AnnouncerType.Main),
                        new DialogueLineEntry("Sucked right into the wrong tunnel! An elimination!", AnnouncerType.Main),
                        new DialogueLineEntry("Explodes out of the chute with tremendous speed.", AnnouncerType.Main),
                        new DialogueLineEntry("Excellent verbal command to pull the dog into the near entrance.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight delay inside the tunnel—perhaps a momentary loss of footing.", AnnouncerType.Main),
                        new DialogueLineEntry("Pops out and immediately acquires the next target.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler needs to hustle to beat the dog to the exit.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("If he goes in there and doesn't come out, do they send in another dog to find him?", AnnouncerType.Color),
                        new DialogueLineEntry("I got stuck in a sleeping bag like that once. Panic sets in fast, let me tell you.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, do you think there are treats in there? Or maybe a small, terrified rabbit?", AnnouncerType.Color),
                        new DialogueLineEntry("Like toothpaste out of a tube! Just squirted right out!", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if he thinks he's digging to China. Let me know if he brings back a souvenir.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f }
                }
            };
        }
        
        private static void PopulateTeeterTotterPool(BestInShowDialogue dialogue)
        {
            dialogue.teeterTotterPool = new CommentaryStatePool
            {
                state = CommentaryState.TeeterTotter,
                arthurDelay = 0f,
                buckDelay = 0.5f,
                buckChance = 0.4f,
                arthurPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("Up onto the teeter...", AnnouncerType.Main),
                        new DialogueLineEntry("Waits for the drop... excellent patience.", AnnouncerType.Main),
                        new DialogueLineEntry("A bit of a fly-off! The board didn't hit the ground before the dog left.", AnnouncerType.Main),
                        new DialogueLineEntry("That's a fault for a fly-off.", AnnouncerType.Main),
                        new DialogueLineEntry("Rides the board down smoothly.", AnnouncerType.Main),
                        new DialogueLineEntry("Heavy impact on the pivot, but the dog handles it.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler supports the dog well through the descent.", AnnouncerType.Main),
                        new DialogueLineEntry("You can see the dog shifting its weight to control the drop speed.", AnnouncerType.Main),
                        new DialogueLineEntry("A very controlled, deliberate teeter performance.", AnnouncerType.Main),
                        new DialogueLineEntry("Bailed off the side! That's a refusal.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1.5f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1.5f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("You ever play on a seesaw with a fat kid, Arthur? Traumatic.", AnnouncerType.Color),
                        new DialogueLineEntry("Boing! Look at him ride that plank. Hang ten, little buddy!", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog is heavier than it looks. Or gravity is acting up again.", AnnouncerType.Color),
                        new DialogueLineEntry("If I put a hat on that dog right now, is it technically a pirate?", AnnouncerType.Color),
                        new DialogueLineEntry("He's waiting for the drop... just like my ex-wife waiting for my alimony check.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1.5f }
                }
            };
        }
        
        private static void PopulateJumpsPool(BestInShowDialogue dialogue)
        {
            dialogue.jumpsPool = new CommentaryStatePool
            {
                state = CommentaryState.Jumps,
                arthurDelay = 0f,
                buckDelay = 0.5f,
                buckChance = 0.4f,
                arthurPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("Clears the tire jump effortlessly.", AnnouncerType.Main),
                        new DialogueLineEntry("Tight wrap around the jump stanchion.", AnnouncerType.Main),
                        new DialogueLineEntry("Dropped the bar! A heartbreaking five faults.", AnnouncerType.Main),
                        new DialogueLineEntry("Took it far too wide, wasting precious yardage.", AnnouncerType.Main),
                        new DialogueLineEntry("Beautiful extension over the broad jump.", AnnouncerType.Main),
                        new DialogueLineEntry("A slicing path over the triple spread.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler signals the turn while the dog is still in the air.", AnnouncerType.Main),
                        new DialogueLineEntry("Knocked the upright. That's a sloppy approach.", AnnouncerType.Main),
                        new DialogueLineEntry("Perfect collection before the jump.", AnnouncerType.Main),
                        new DialogueLineEntry("Sailing over the final oxer!", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1.5f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("I once saw a guy eat a tire like that. Course, it was made of black licorice.", AnnouncerType.Color),
                        new DialogueLineEntry("Why don't they just fly over it? Oh, right. Dogs. I forgot.", AnnouncerType.Color),
                        new DialogueLineEntry("Look at the legs on that one. Think he could play shortstop for the Mets?", AnnouncerType.Color),
                        new DialogueLineEntry("He knocked it down! Can we get a carpenter out here? This place is falling apart.", AnnouncerType.Color),
                        new DialogueLineEntry("I've jumped out of windows higher than that, Arthur. Mostly for romantic reasons.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1.5f, 1f }
                }
            };
        }
        
        private static void PopulateMistakesPool(BestInShowDialogue dialogue)
        {
            dialogue.mistakesPool = new CommentaryStatePool
            {
                state = CommentaryState.Mistakes,
                arthurDelay = 0f,
                buckDelay = 0.3f,
                buckChance = 0.5f,
                arthurPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("Oh no, the dog is completely off course.", AnnouncerType.Main),
                        new DialogueLineEntry("A refusal at the A-frame. That's a five-point deduction.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler has completely lost the dog's focus.", AnnouncerType.Main),
                        new DialogueLineEntry("Spinning in circles. The connection between handler and dog is broken.", AnnouncerType.Main),
                        new DialogueLineEntry("Took the wrong obstacle! That is an instant elimination.", AnnouncerType.Main),
                        new DialogueLineEntry("Stopping to sniff the turf. A disaster for their time.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler is frantically trying to recall.", AnnouncerType.Main),
                        new DialogueLineEntry("Crossed behind the dog, drawing them off the line.", AnnouncerType.Main),
                        new DialogueLineEntry("A very costly miscommunication there.", AnnouncerType.Main),
                        new DialogueLineEntry("That run has completely unraveled.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1.5f, 1f, 1f, 2f, 1.5f, 1f, 1f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("He's sniffing the ground! I think he found oil, Arthur. We're rich!", AnnouncerType.Color),
                        new DialogueLineEntry("The dog is looking at me. Stop looking at me! I don't have the jerky anymore!", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, what if the dog just speaks Spanish and doesn't understand the lady?", AnnouncerType.Color),
                        new DialogueLineEntry("He's making his own course. I respect that. A true artist doesn't follow lines.", AnnouncerType.Color),
                        new DialogueLineEntry("Is there a penalty if the dog just goes over to the crowd and asks for a hot dog?", AnnouncerType.Color),
                        new DialogueLineEntry("Well, that was a trainwreck. Should we go down there and help? I have a whistle.", AnnouncerType.Color),
                        new DialogueLineEntry("The dog is looking at the handler like she owes him money.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1.5f, 1f, 1f, 1f, 1f, 1f, 1f }
                }
            };
        }
        
        private static void PopulateFinishLinePool(BestInShowDialogue dialogue)
        {
            dialogue.finishLinePool = new CommentaryStatePool
            {
                state = CommentaryState.FinishLine,
                arthurDelay = 0f,
                buckDelay = 0.5f,
                buckChance = 0.5f,
                arthurPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("Over the final jump and across the line!", AnnouncerType.Main),
                        new DialogueLineEntry("Stops the clock at a spectacular time.", AnnouncerType.Main),
                        new DialogueLineEntry("A completely clean run. A masterclass in agility.", AnnouncerType.Main),
                        new DialogueLineEntry("They will be thrilled with that performance.", AnnouncerType.Main),
                        new DialogueLineEntry("Unfortunately, those faults will keep them off the podium today.", AnnouncerType.Main),
                        new DialogueLineEntry("Let's look at the replay of that front cross.", AnnouncerType.Main),
                        new DialogueLineEntry("A valiant effort, but just a fraction off the pace.", AnnouncerType.Main),
                        new DialogueLineEntry("The crowd is on their feet!", AnnouncerType.Main),
                        new DialogueLineEntry("That is why they are the defending champions.", AnnouncerType.Main),
                        new DialogueLineEntry("Thank you for joining us for this magnificent display of canine athleticism.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1.5f, 1.5f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        new DialogueLineEntry("He did it! Does he get a giant novelty check? Dogs love chewing on cardboard.", AnnouncerType.Color),
                        new DialogueLineEntry("I'm exhausted just watching that. Arthur, carry me to the car.", AnnouncerType.Color),
                        new DialogueLineEntry("You know, at the end of the day, it's just a wolf running over a piece of wood.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder what that dog's going to do to celebrate. Probably drink out of the toilet.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, tell the truth: Are any of these dogs wearing a wire? It feels rigged.", AnnouncerType.Color),
                        new DialogueLineEntry("I'm going down to the floor to see if I can adopt the loser. I like an underdog.", AnnouncerType.Color),
                        new DialogueLineEntry("Goodnight everyone! Remember to spay and neuter your commentators!", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 2f }
                }
            };
        }
        
        #endregion
    }
}
#endif
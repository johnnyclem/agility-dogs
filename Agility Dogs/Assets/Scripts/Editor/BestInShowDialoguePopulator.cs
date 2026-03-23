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
                        // Original 10 lines
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
                        // Additional Arthur lines (Match Intro)
                        new DialogueLineEntry("This is it, folks. The culmination of months of training and dedication.", AnnouncerType.Main),
                        new DialogueLineEntry("Watch for the front cross at obstacle three. That's where this course will be won or lost.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's focus is remarkable. You can see the intensity in those eyes.", AnnouncerType.Main),
                        new DialogueLineEntry("A beautiful starting position. Perfect heel position, maximum readiness.", AnnouncerType.Main),
                        new DialogueLineEntry("The course designer has really outdone themselves today. Technical, flowing, unforgiving.", AnnouncerType.Main),
                        new DialogueLineEntry("We've seen some incredible times in qualifying. The bar has been raised.", AnnouncerType.Main),
                        new DialogueLineEntry("The bond between handler and dog is evident. Years of partnership on display.", AnnouncerType.Main),
                        new DialogueLineEntry("Watch the handler's body language. Every subtle movement communicates to the dog.", AnnouncerType.Main),
                        new DialogueLineEntry("This venue has seen some of the greatest runs in agility history.", AnnouncerType.Main),
                        new DialogueLineEntry("The atmosphere is electric. You can feel the tension in the arena.", AnnouncerType.Main),
                        new DialogueLineEntry("A moment of calm before the storm. Both handler and dog centering themselves.", AnnouncerType.Main),
                        new DialogueLineEntry("The judge signals. We are moments away from the start.", AnnouncerType.Main),
                        new DialogueLineEntry("Obstacle one is a simple bar jump, but it sets the tone for the entire run.", AnnouncerType.Main),
                        new DialogueLineEntry("Notice how the handler positions herself for an optimal start.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's tail is wagging. Eager and ready for the challenge ahead.", AnnouncerType.Main),
                        new DialogueLineEntry("This is championship caliber agility. Every second counts.", AnnouncerType.Main),
                        new DialogueLineEntry("The course map shows seventeen obstacles. A true test of speed and precision.", AnnouncerType.Main),
                        new DialogueLineEntry("A veteran team here. You can see the experience in their preparation.", AnnouncerType.Main),
                        new DialogueLineEntry("The crowd falls silent. Respect for the competitors about to perform.", AnnouncerType.Main),
                        new DialogueLineEntry("Watch for the blind cross at the tunnel. That's the signature move of this handler.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog shakes off, a sign of readiness. Game time.", AnnouncerType.Main),
                        new DialogueLineEntry("We've got a treat in store today. Two of the best teams in the world.", AnnouncerType.Main),
                        new DialogueLineEntry("The weave poles at position eight will be crucial. Entry angle is tricky.", AnnouncerType.Main),
                        new DialogueLineEntry("A moment of connection between handler and dog. Beautiful to witness.", AnnouncerType.Main),
                        new DialogueLineEntry("The timer is ready. The judge is in position. Let's go!", AnnouncerType.Main),
                        new DialogueLineEntry("This course rewards bravery. You have to commit to your lines.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler adjusts her watch. Precision in every detail.", AnnouncerType.Main),
                        new DialogueLineEntry("An immaculate day for agility. Perfect conditions for a fast run.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's ears are forward. Alert, attentive, ready to explode.", AnnouncerType.Main),
                        new DialogueLineEntry("We're looking at a potential course record here, folks.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        // Original 6 lines
                        new DialogueLineEntry("Arthur, how much do you think that dog owes in taxes?", AnnouncerType.Color),
                        new DialogueLineEntry("I'm looking at the handler, Arthur. She's got great calves. Think she's single?", AnnouncerType.Color),
                        new DialogueLineEntry("If I were a dog, Arthur, I'd just bite the judge and go home.", AnnouncerType.Color),
                        new DialogueLineEntry("Do you think these dogs know they aren't tiny horses? Or do they think we're giant dogs?", AnnouncerType.Color),
                        new DialogueLineEntry("I brought a bag of beef jerky. You think they can smell it from the booth? Watch this.", AnnouncerType.Color),
                        new DialogueLineEntry("I've been doing my stretches, Arthur. If a handler goes down, I'm ready to tag in.", AnnouncerType.Color),
                        // Additional Buck lines (Match Intro)
                        new DialogueLineEntry("That dog looks at me, Arthur. He knows I have the good treats.", AnnouncerType.Color),
                        new DialogueLineEntry("I could totally do this. How hard can it be? You just run and point, right?", AnnouncerType.Color),
                        new DialogueLineEntry("My ex-wife had a Chihuahua that could clear a fence. Angry little thing.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I just realized something. We're just watching dogs run around. This is my dream job.", AnnouncerType.Color),
                        new DialogueLineEntry("I tried agility once with my buddy's Golden Retriever. We both ended up in the hospital.", AnnouncerType.Color),
                        new DialogueLineEntry("These handlers spend more time with their dogs than their spouses. I respect that.", AnnouncerType.Color),
                        new DialogueLineEntry("You know what this sport needs? More fireworks. Every run should end with fireworks.", AnnouncerType.Color),
                        new DialogueLineEntry("I once saw a Pomeranian beat a German Shepherd. Never underestimate the little guys.", AnnouncerType.Color),
                        new DialogueLineEntry("Is there a beer garden here? Asking for research purposes.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, be honest. Could you pass a basic obedience test? I couldn't.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has better posture than I do. And I'm sitting in a chair.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if the dogs think we're weird for watching them run in circles.", AnnouncerType.Color),
                        new DialogueLineEntry("My doctor said I need more exercise. Watching this should count, right?", AnnouncerType.Color),
                        new DialogueLineEntry("I bet that dog has a better social media following than both of us combined.", AnnouncerType.Color),
                        new DialogueLineEntry("If I had a dog like that, I'd just stay home and play video games. Good boy!", AnnouncerType.Color),
                        new DialogueLineEntry("This is like watching NASCAR but the cars are cute and furry.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, do you think dogs dream about agility? Like, flying over jumps in slow motion?", AnnouncerType.Color),
                        new DialogueLineEntry("I'm getting emotional, Arthur. Look at that bond. That's love right there.", AnnouncerType.Color),
                        new DialogueLineEntry("That handler's outfit cost more than my car. And my car has heated seats.", AnnouncerType.Color),
                        new DialogueLineEntry("I should get into this sport. I'm already good at running away from responsibility.", AnnouncerType.Color),
                        new DialogueLineEntry("These dogs are athletes, Arthur. Better athletes than half the NFL.", AnnouncerType.Color),
                        new DialogueLineEntry("You know what would make this better? A halftime show. Maybe some dog tricks.", AnnouncerType.Color),
                        new DialogueLineEntry("I brought binoculars. Not for the dogs. For the snacks in the concession stand.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I just made sixty dollars betting with the cameraman. Don't tell anyone.", AnnouncerType.Color),
                        new DialogueLineEntry("I think that dog just winked at me. We have a connection, Arthur.", AnnouncerType.Color),
                        new DialogueLineEntry("This is way better than golf. And you can pet the players.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if they hire announcers who are also willing to walk the dogs. Asking for me.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
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
                        // Original 10 lines
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
                        // Additional Arthur lines (Weave Poles)
                        new DialogueLineEntry("Twelve poles, twelve entries. A true test of training and muscle memory.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's shoulders are doing all the work. Incredible upper body strength.", AnnouncerType.Main),
                        new DialogueLineEntry("Perfect rhythm. You could set a metronome to that pace.", AnnouncerType.Main),
                        new DialogueLineEntry("The entry angle was tight, but the dog adjusted beautifully.", AnnouncerType.Main),
                        new DialogueLineEntry("Watch the rear legs. They're driving the dog forward with each weave.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight drift to the left, but they recover without losing momentum.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler is already signaling the next obstacle. Brilliant anticipation.", AnnouncerType.Main),
                        new DialogueLineEntry("That's the fastest weave time I've seen all tournament.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's focus is laser-sharp. Not a single wasted movement.", AnnouncerType.Main),
                        new DialogueLineEntry("A minor bobble on pole eight, but overall a stellar performance.", AnnouncerType.Main),
                        new DialogueLineEntry("Notice how the dog keeps its head low. Optimal weaving posture.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler maintains eye contact throughout. A beautiful partnership.", AnnouncerType.Main),
                        new DialogueLineEntry("Six poles in and still maintaining speed. Impressive conditioning.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog pops out cleanly. No hesitation on the exit.", AnnouncerType.Main),
                        new DialogueLineEntry("A textbook example of weave pole technique. Young handlers take note.", AnnouncerType.Main),
                        new DialogueLineEntry("The rhythm is mesmerizing. Left, right, left, right. Poetry in motion.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's tail acts as a counterbalance. Nature's design at work.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight deceleration on the final poles, but still within acceptable time.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's body position encourages forward drive. Excellent technique.", AnnouncerType.Main),
                        new DialogueLineEntry("That entry was razor-thin. The margin for error is almost zero.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's agility through the poles is a testament to months of practice.", AnnouncerType.Main),
                        new DialogueLineEntry("Weave poles are the great separator. This dog is clearly elite.", AnnouncerType.Main),
                        new DialogueLineEntry("A clean exit sets up perfectly for the next obstacle.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's speed through the weaves is adding valuable seconds to their time.", AnnouncerType.Main),
                        new DialogueLineEntry("Notice the handler's quiet hands. No unnecessary motion to distract the dog.", AnnouncerType.Main),
                        new DialogueLineEntry("A flawless weave performance. Exactly what you want to see at this level.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's body undulates through the poles like water through rocks.", AnnouncerType.Main),
                        new DialogueLineEntry("Every pole hit perfectly. That's precision training right there.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's timing is impeccable, supporting without overwhelming.", AnnouncerType.Main),
                        new DialogueLineEntry("A masterclass in weave pole execution. Championship caliber.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        // Original 5 lines
                        new DialogueLineEntry("He's weaving like me leaving the pub on a Tuesday!", AnnouncerType.Color),
                        new DialogueLineEntry("I tried that once after a few margaritas. Broke a lamp.", AnnouncerType.Color),
                        new DialogueLineEntry("You know, if you tape a broom to a dog's tail doing that, you'd never have to sweep.", AnnouncerType.Color),
                        new DialogueLineEntry("Why don't they just go in a straight line, Arthur? Seems highly inefficient.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog is vibrating. Have they been giving them espresso?", AnnouncerType.Color),
                        // Additional Buck lines (Weave Poles)
                        new DialogueLineEntry("I'm getting dizzy watching this. How are they not throwing up?", AnnouncerType.Color),
                        new DialogueLineEntry("This looks like me trying to walk straight after Thanksgiving dinner.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I think the dog is doing the limbo. Is that legal?", AnnouncerType.Color),
                        new DialogueLineEntry("My cousin tried weave poles with a garden hose. It did not end well.", AnnouncerType.Color),
                        new DialogueLineEntry("This is like watching someone parallel park but fast and with paws.", AnnouncerType.Color),
                        new DialogueLineEntry("If I could weave like that, I'd never need to use a revolving door again.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog's hips don't lie. Shakira would be jealous.", AnnouncerType.Color),
                        new DialogueLineEntry("I want to see a human do this. Pay-per-view, Arthur. We'd make millions.", AnnouncerType.Color),
                        new DialogueLineEntry("This is mesmerizing. I could watch this all day. Do they have weave pole ASMR?", AnnouncerType.Color),
                        new DialogueLineEntry("I tried to weave through a crowd like that at the mall. Got tackled by security.", AnnouncerType.Color),
                        new DialogueLineEntry("The dog is doing the snake! The dog is doing the snake!", AnnouncerType.Color),
                        new DialogueLineEntry("I think the poles are moving. Is this a magic show or a dog show?", AnnouncerType.Color),
                        new DialogueLineEntry("This is better than my DDR skills in college. And I was a legend.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I just realized something. This dog has better coordination than I do.", AnnouncerType.Color),
                        new DialogueLineEntry("I want to attach a GoPro to that dog. YouTube gold, baby!", AnnouncerType.Color),
                        new DialogueLineEntry("This is like a slalom but for dogs. Downhill dog racing. Coming to ESPN.", AnnouncerType.Color),
                        new DialogueLineEntry("If that dog ever gets tired, I'll lend him my back. It's already crooked.", AnnouncerType.Color),
                        new DialogueLineEntry("I need this dog to teach me how to navigate the singles scene.", AnnouncerType.Color),
                        new DialogueLineEntry("That's the S-work right there. Smooth, sexy, and slightly confusing.", AnnouncerType.Color),
                        new DialogueLineEntry("I bet this dog could thread a needle. With its paws. And eyes closed.", AnnouncerType.Color),
                        new DialogueLineEntry("The dog is vibing. The dog is absolutely vibing right now.", AnnouncerType.Color),
                        new DialogueLineEntry("I tried to do this with a hula hoop once. I'm still in physical therapy.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, this dog is flexing on us. He knows he's good and he's not sorry.", AnnouncerType.Color),
                        new DialogueLineEntry("This is hypnotic. I think the dog is casting a spell on the audience.", AnnouncerType.Color),
                        new DialogueLineEntry("I need that dog's core strength routine. Asking for a friend. The friend is me.", AnnouncerType.Color),
                        new DialogueLineEntry("If this dog ever retires, he should be a weave pole coach. Elite.", AnnouncerType.Color),
                        new DialogueLineEntry("I'm seeing double now. The poles, the dog, everything is spinning.", AnnouncerType.Color),
                        new DialogueLineEntry("That's some Mario Kart drifting right there. Blue shell dodged!", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
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
                        // Original 10 lines
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
                        // Additional Arthur lines (Contact Obstacles)
                        new DialogueLineEntry("The dog drives up the A-frame with tremendous power.", AnnouncerType.Main),
                        new DialogueLineEntry("Watch the contact zone. The dog must touch with at least one paw.", AnnouncerType.Main),
                        new DialogueLineEntry("A running contact! The dog maintains speed through the yellow zone.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's timing on the release command is impeccable.", AnnouncerType.Main),
                        new DialogueLineEntry("On the dog walk now. A test of balance and confidence.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's body is perfectly aligned on the narrow plank.", AnnouncerType.Main),
                        new DialogueLineEntry("Excellent speed on the up ramp, but controlled on the descent.", AnnouncerType.Main),
                        new DialogueLineEntry("The judge is watching closely. Every contact zone matters.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight wobble, but the dog maintains its line. Impressive.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's confidence on contact obstacles is a sign of excellent training.", AnnouncerType.Main),
                        new DialogueLineEntry("Notice how the dog looks back for the handler at the apex.", AnnouncerType.Main),
                        new DialogueLineEntry("A textbook two-on-two-off. Both rear paws on the contact zone.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog clears the dog walk in record time.", AnnouncerType.Main),
                        new DialogueLineEntry("The contact zone precision here is championship level.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler uses a verbal release to signal the dog's departure.", AnnouncerType.Main),
                        new DialogueLineEntry("A beautiful arc over the A-frame apex. Maximum efficiency.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's descent is controlled and deliberate. Smart running.", AnnouncerType.Main),
                        new DialogueLineEntry("Contact obstacles separate the elite from the rest.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's paws hit the yellow zone with surgical precision.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight hesitation, but within acceptable time loss.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog walks the plank with the confidence of a tightrope walker.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's position encourages a fast but safe descent.", AnnouncerType.Main),
                        new DialogueLineEntry("This dog has mastered the contact zones. Years of practice evident.", AnnouncerType.Main),
                        new DialogueLineEntry("The judge signals. Clean contact confirmed.", AnnouncerType.Main),
                        new DialogueLineEntry("A perfect run through the contact obstacles so far.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's training shines through on these technical obstacles.", AnnouncerType.Main),
                        new DialogueLineEntry("Watch the paws. That's where the points are won or lost.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's body position ensures no wasted motion.", AnnouncerType.Main),
                        new DialogueLineEntry("Contact obstacles are where champions are made.", AnnouncerType.Main),
                        new DialogueLineEntry("A masterclass in contact zone execution.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1.5f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        // Original 5 lines
                        new DialogueLineEntry("I climbed an A-frame once. Had to call the fire department to get me down.", AnnouncerType.Color),
                        new DialogueLineEntry("If he slips, does he get workers' comp?", AnnouncerType.Color),
                        new DialogueLineEntry("That ramp looks like the roof of my first apartment.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, what happens if the dog just decides to take a nap up there?", AnnouncerType.Color),
                        new DialogueLineEntry("I bet you could launch a pretty good firework off the top of that thing.", AnnouncerType.Color),
                        // Additional Buck lines (Contact Obstacles)
                        new DialogueLineEntry("I tried to walk across a balance beam once. I'm still in therapy.", AnnouncerType.Color),
                        new DialogueLineEntry("That A-frame looks like something my gym teacher would make me climb.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, do you think the dog gets vertigo up there? I would.", AnnouncerType.Color),
                        new DialogueLineEntry("If I had to do that, I'd need a harness, a helmet, and a life alert button.", AnnouncerType.Color),
                        new DialogueLineEntry("I just realized something. This dog has better balance than I have in my entire life.", AnnouncerType.Color),
                        new DialogueLineEntry("That ramp is longer than my last relationship, and way more stable.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if the dog charges extra for the contact zone. Premium service!", AnnouncerType.Color),
                        new DialogueLineEntry("I tried to do the dog walk at a playground once. The fire department came again.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, that dog is walking like me after leg day at the gym.", AnnouncerType.Color),
                        new DialogueLineEntry("If that dog slips, does the crowd throw a rope? Safety first!", AnnouncerType.Color),
                        new DialogueLineEntry("This looks like an obstacle course from a reality TV show.", AnnouncerType.Color),
                        new DialogueLineEntry("I need that dog's insurance plan. Contact zone coverage, please.", AnnouncerType.Color),
                        new DialogueLineEntry("I once fell off a seesaw at a birthday party. Never recovered emotionally.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog is smoother than a used car salesman on commission.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I'm getting nervous just watching this. My palms are sweaty.", AnnouncerType.Color),
                        new DialogueLineEntry("I bet that dog could cross a frozen lake without cracking the ice.", AnnouncerType.Color),
                        new DialogueLineEntry("This is like watching parkour but with more fur and less pretension.", AnnouncerType.Color),
                        new DialogueLineEntry("If I had those contact skills, I'd never spill my coffee again.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has better footwork than my dance instructor. And she charges $200 an hour.", AnnouncerType.Color),
                        new DialogueLineEntry("I want to see the dog do this with roller skates. Pay-per-view event!", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, that dog is living his best life up there. Peak performance.", AnnouncerType.Color),
                        new DialogueLineEntry("I think I just pulled a muscle watching that. Is that possible?", AnnouncerType.Color),
                        new DialogueLineEntry("That contact zone should have a Yelp review. Five stars!", AnnouncerType.Color),
                        new DialogueLineEntry("I need to hire this dog to walk across my roof and check for leaks.", AnnouncerType.Color),
                        new DialogueLineEntry("This is the most intense thing I've watched since my last divorce hearing.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
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
                        // Original 10 lines
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
                        // Additional Arthur lines (Tunnel)
                        new DialogueLineEntry("The tunnel is fully enclosed. The dog runs on pure instinct and training.", AnnouncerType.Main),
                        new DialogueLineEntry("A curved tunnel entry. The dog must commit without seeing the exit.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler positions for a front cross while the dog navigates blind.", AnnouncerType.Main),
                        new DialogueLineEntry("Speed through the tunnel is critical. Every tenth counts.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's exit angle will determine the next obstacle approach.", AnnouncerType.Main),
                        new DialogueLineEntry("A tunnel-to-tunnel sequence. The dog must differentiate between entries.", AnnouncerType.Main),
                        new DialogueLineEntry("The fabric rustles as the dog blasts through. Pure speed.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's verbal cue at the entrance was perfectly timed.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight hesitation at the exit. The dog reorients to the handler.", AnnouncerType.Main),
                        new DialogueLineEntry("Tunnel speed is a sign of confidence. This dog has it in spades.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's body low to the ground inside the tunnel. Optimal form.", AnnouncerType.Main),
                        new DialogueLineEntry("A smooth transition from tunnel to the next obstacle.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's position at the exit draws the dog forward.", AnnouncerType.Main),
                        new DialogueLineEntry("Tunnels are about trust. The dog trusts the handler's guidance.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog accelerates through the tunnel without hesitation.", AnnouncerType.Main),
                        new DialogueLineEntry("A brilliant blind cross executed while the dog was out of sight.", AnnouncerType.Main),
                        new DialogueLineEntry("The tunnel entry was tight, but the dog committed fully.", AnnouncerType.Main),
                        new DialogueLineEntry("Exit speed is impressive. The dog explodes out of the chute.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler barely beats the dog to the exit. Perfect timing.", AnnouncerType.Main),
                        new DialogueLineEntry("Tunnel navigation is second nature to this experienced dog.", AnnouncerType.Main),
                        new DialogueLineEntry("The curved tunnel tests the dog's ability to maintain momentum.", AnnouncerType.Main),
                        new DialogueLineEntry("A clean exit sets up the next obstacle perfectly.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's commitment to the tunnel entry was decisive.", AnnouncerType.Main),
                        new DialogueLineEntry("Tunnels add variety and challenge to any agility course.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's exit was perfectly aligned with the handler's position.", AnnouncerType.Main),
                        new DialogueLineEntry("A textbook tunnel run. No wasted motion, maximum speed.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's body language guided the dog through without verbal cues.", AnnouncerType.Main),
                        new DialogueLineEntry("Tunnel performance like this separates good teams from great ones.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's confidence in tunnels is evident. Full speed ahead.", AnnouncerType.Main),
                        new DialogueLineEntry("A flawless tunnel execution. Championship caliber.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        // Original 5 lines
                        new DialogueLineEntry("If he goes in there and doesn't come out, do they send in another dog to find him?", AnnouncerType.Color),
                        new DialogueLineEntry("I got stuck in a sleeping bag like that once. Panic sets in fast, let me tell you.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, do you think there are treats in there? Or maybe a small, terrified rabbit?", AnnouncerType.Color),
                        new DialogueLineEntry("Like toothpaste out of a tube! Just squirted right out!", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if he thinks he's digging to China. Let me know if he brings back a souvenir.", AnnouncerType.Color),
                        // Additional Buck lines (Tunnel)
                        new DialogueLineEntry("I tried to crawl through a tunnel at a play place once. Got stuck. Had to call mom.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, what if there's a family of squirrels living in there? Hostile takeover!", AnnouncerType.Color),
                        new DialogueLineEntry("That dog looks like me trying to find the bathroom at a movie theater.", AnnouncerType.Color),
                        new DialogueLineEntry("I bet that dog has nightmares about tunnels. Or maybe dreams. Hard to tell.", AnnouncerType.Color),
                        new DialogueLineEntry("If I went in there, I'd need GPS, a flashlight, and a emotional support animal.", AnnouncerType.Color),
                        new DialogueLineEntry("That tunnel looks like a giant sock. Somebody needs to do laundry.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if the dog can hear the crowd cheering from inside. That would be cool.", AnnouncerType.Color),
                        new DialogueLineEntry("This is like a real-life game of whack-a-mole. Pop goes the doggy!", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I just realized something. I've been inside tunnels at amusement parks. I'm basically an agility dog.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog came out so fast, I thought he was shot out of a cannon.", AnnouncerType.Color),
                        new DialogueLineEntry("I need one of these tunnels for my house. Great for hiding from my ex-wife.", AnnouncerType.Color),
                        new DialogueLineEntry("The dog disappears like a magician. Now you see him, now you don't!", AnnouncerType.Color),
                        new DialogueLineEntry("I bet the dog is singing in there. Solo performance, no audience.", AnnouncerType.Color),
                        new DialogueLineEntry("This is like my dating life. In and out, nobody knows what happened.", AnnouncerType.Color),
                        new DialogueLineEntry("If that dog brings back a stick from China, I'll eat my hat.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I want to crawl in there and just take a nap. It looks cozy.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog runs through tunnels like I run to the fridge at midnight.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog just did a victory lap inside. That was way too long.", AnnouncerType.Color),
                        new DialogueLineEntry("The tunnel is basically a giant chew toy. Where does it end? Nobody knows.", AnnouncerType.Color),
                        new DialogueLineEntry("I'm claustrophobic, Arthur. Just watching this is giving me anxiety.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has better tunnel vision than me trying to find a parking spot.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog is doing parkour in there. We just can't see it.", AnnouncerType.Color),
                        new DialogueLineEntry("If that tunnel had WiFi, the dog would never come out.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if the dog leaves little presents inside. Like a trail of breadcrumbs.", AnnouncerType.Color),
                        new DialogueLineEntry("This is better than any roller coaster. No lines, no tickets, just pure chaos.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
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
                        // Original 10 lines
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
                        // Additional Arthur lines (Teeter-Totter)
                        new DialogueLineEntry("The teeter tests the dog's confidence and balance.", AnnouncerType.Main),
                        new DialogueLineEntry("A perfect ride down. The board touches as the dog exits.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's timing on the pivot point is critical.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight hesitation at the apex. The dog waits for the board to drop.", AnnouncerType.Main),
                        new DialogueLineEntry("The teeter's pivot point requires the dog to shift weight dynamically.", AnnouncerType.Main),
                        new DialogueLineEntry("Excellent contact zone discipline on the teeter descent.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's rear paws remain on the board until it touches down.", AnnouncerType.Main),
                        new DialogueLineEntry("A confident teeter performance. No fear evident.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's calm demeanor helps the dog stay focused.", AnnouncerType.Main),
                        new DialogueLineEntry("The teeter's bang on touchdown can startle inexperienced dogs.", AnnouncerType.Main),
                        new DialogueLineEntry("A controlled descent. The dog manages the board's momentum.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog rides the teeter like a seasoned pro.", AnnouncerType.Main),
                        new DialogueLineEntry("A textbook teeter execution. Nothing to fault here.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's balance on the teeter is remarkable.", AnnouncerType.Main),
                        new DialogueLineEntry("A smooth transition from ascent to descent.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's verbal cue at the pivot point was perfectly timed.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's weight shift controls the teeter's drop speed.", AnnouncerType.Main),
                        new DialogueLineEntry("A flawless teeter run. The judges will be pleased.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's confidence on the teeter shows excellent training.", AnnouncerType.Main),
                        new DialogueLineEntry("A proper teeter requires patience and precision.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog waits for the board to settle before exiting.", AnnouncerType.Main),
                        new DialogueLineEntry("A beautiful teeter performance. Textbook execution.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's rear feet stay planted until the board touches.", AnnouncerType.Main),
                        new DialogueLineEntry("A controlled, deliberate teeter run.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's body position encourages forward drive.", AnnouncerType.Main),
                        new DialogueLineEntry("Teeter mastery separates the contenders from the pretenders.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's timing at the pivot is impeccable.", AnnouncerType.Main),
                        new DialogueLineEntry("A championship-level teeter performance.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's balance and timing are in perfect harmony.", AnnouncerType.Main),
                        new DialogueLineEntry("A flawless teeter execution. Nothing left to chance.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1.5f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        // Original 5 lines
                        new DialogueLineEntry("You ever play on a seesaw with a fat kid, Arthur? Traumatic.", AnnouncerType.Color),
                        new DialogueLineEntry("Boing! Look at him ride that plank. Hang ten, little buddy!", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog is heavier than it looks. Or gravity is acting up again.", AnnouncerType.Color),
                        new DialogueLineEntry("If I put a hat on that dog right now, is it technically a pirate?", AnnouncerType.Color),
                        new DialogueLineEntry("He's waiting for the drop... just like my ex-wife waiting for my alimony check.", AnnouncerType.Color),
                        // Additional Buck lines (Teeter-Totter)
                        new DialogueLineEntry("I rode a teeter-totter once and the other kid flew off. I'm still processing.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, do you think the dog enjoys this? Or is he just doing it for the treats?", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has better core strength than my entire gym membership.", AnnouncerType.Color),
                        new DialogueLineEntry("If I tried that, I'd need a crash pad, a helmet, and a will signed in triplicate.", AnnouncerType.Color),
                        new DialogueLineEntry("The teeter is basically a torture device designed for dogs. Brutal.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog is doing yoga. Downward dog, teeter edition.", AnnouncerType.Color),
                        new DialogueLineEntry("If that board could talk, it would say 'Please, no more dogs.'", AnnouncerType.Color),
                        new DialogueLineEntry("I tried to balance on a beam once. My chiropractor still sends me Christmas cards.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog rides the teeter like a pro surfer. Hang ten, little dude!", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if the dog gets dizzy doing this. I'm getting dizzy watching.", AnnouncerType.Color),
                        new DialogueLineEntry("The teeter is basically a see-saw of destiny. Will he fall? Will he fly?", AnnouncerType.Color),
                        new DialogueLineEntry("If that dog played drums, he'd have perfect timing on the cymbal crashes.", AnnouncerType.Color),
                        new DialogueLineEntry("I want to see a human do this. Pay-per-view, Arthur. We'd be rich.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog's balance is better than my emotional stability.", AnnouncerType.Color),
                        new DialogueLineEntry("I need to hire that dog to walk on my roof and fix the gutters.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I just realized something. I've been teetering my whole life.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog is riding the teeter like a mechanical bull at a rodeo.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog is trying to communicate in Morse code with the board.", AnnouncerType.Color),
                        new DialogueLineEntry("If the teeter had a speaker, it would be playing 'Eye of the Tiger.'", AnnouncerType.Color),
                        new DialogueLineEntry("I tried to do a handstand once. My face still hurts thinking about it.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has the balance of a tightrope walker and the courage of a lion.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if the teeter is a stress reliever for the dog. Smash, smash, smash!", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, that dog is living his best life. Teetering on the edge of greatness.", AnnouncerType.Color),
                        new DialogueLineEntry("I need one of these in my office. Great for stress relief during meetings.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has better pivot skills than a basketball player.", AnnouncerType.Color),
                        new DialogueLineEntry("If I had that balance, I'd never trip over my own feet again. A man can dream.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
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
                        // Original 10 lines
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
                        // Additional Arthur lines (Jumps)
                        new DialogueLineEntry("A textbook jump. The dog clears the bar with inches to spare.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's motion encourages a tight turn over the jump.", AnnouncerType.Main),
                        new DialogueLineEntry("A slight brush on the bar, but it stays up. Lucky.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's arc over the jump is perfectly calculated.", AnnouncerType.Main),
                        new DialogueLineEntry("A refusal! The dog bypassed the jump entirely.", AnnouncerType.Main),
                        new DialogueLineEntry("Excellent timing on the handler's command. The dog commits early.", AnnouncerType.Main),
                        new DialogueLineEntry("The double jump requires precise timing and elevation.", AnnouncerType.Main),
                        new DialogueLineEntry("A slice turn over the bar. Maximum efficiency.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's jumping technique is a thing of beauty.", AnnouncerType.Main),
                        new DialogueLineEntry("A dropped bar. That's five faults added to the score.", AnnouncerType.Main),
                        new DialogueLineEntry("The triple spread tests the dog's range and confidence.", AnnouncerType.Main),
                        new DialogueLineEntry("A tight wrap around the jump. The dog responds to the handler's cue.", AnnouncerType.Main),
                        new DialogueLineEntry("The tire jump requires precision. The dog threads the needle.", AnnouncerType.Main),
                        new DialogueLineEntry("A beautiful broad jump. The dog extends fully.", AnnouncerType.Main),
                        new DialogueLineEntry("The wall jump is a test of vertical leap.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's takeoff point was perfect. Maximum clearance.", AnnouncerType.Main),
                        new DialogueLineEntry("A jumping sequence executed flawlessly.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's timing sets up the dog for success.", AnnouncerType.Main),
                        new DialogueLineEntry("A clean jump. No wasted motion, no faults.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's form over the jump is textbook.", AnnouncerType.Main),
                        new DialogueLineEntry("A slicing path minimizes distance traveled.", AnnouncerType.Main),
                        new DialogueLineEntry("The judge's eye is on every jump. Precision matters.", AnnouncerType.Main),
                        new DialogueLineEntry("A committed approach. The dog never hesitated.", AnnouncerType.Main),
                        new DialogueLineEntry("The jump sequence flows beautifully.", AnnouncerType.Main),
                        new DialogueLineEntry("A tight turn sets up the next jump perfectly.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's jumping ability is a sight to behold.", AnnouncerType.Main),
                        new DialogueLineEntry("A championship-level jump sequence.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog's athleticism shines over the bars.", AnnouncerType.Main),
                        new DialogueLineEntry("A flawless jumping performance.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler and dog move as one over the jumps.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        // Original 5 lines
                        new DialogueLineEntry("I once saw a guy eat a tire like that. Course, it was made of black licorice.", AnnouncerType.Color),
                        new DialogueLineEntry("Why don't they just fly over it? Oh, right. Dogs. I forgot.", AnnouncerType.Color),
                        new DialogueLineEntry("Look at the legs on that one. Think he could play shortstop for the Mets?", AnnouncerType.Color),
                        new DialogueLineEntry("He knocked it down! Can we get a carpenter out here? This place is falling apart.", AnnouncerType.Color),
                        new DialogueLineEntry("I've jumped out of windows higher than that, Arthur. Mostly for romantic reasons.", AnnouncerType.Color),
                        // Additional Buck lines (Jumps)
                        new DialogueLineEntry("I tried to jump over a fence once. Got stuck at the top. Like a piñata.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, that dog has serious ups. Better than my vertical in high school.", AnnouncerType.Color),
                        new DialogueLineEntry("If I could jump like that, I'd never wait in line again. Just hop right over!", AnnouncerType.Color),
                        new DialogueLineEntry("That dog's jumping is better than my jump shots. And I played basketball.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if the dog thinks the bars are snakes. 'Don't touch the danger sticks!'", AnnouncerType.Color),
                        new DialogueLineEntry("I tried to do a broad jump at the beach once. Pulled a hamstring. Worth it.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has the spring-loaded legs of a cartoon character.", AnnouncerType.Color),
                        new DialogueLineEntry("If I jumped like that, my knees would file a workers' comp claim.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I think the dog is part kangaroo. Look at those hops!", AnnouncerType.Color),
                        new DialogueLineEntry("I once tried to jump a tire. Got my foot stuck. Fire department had to come.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog makes jumping look easy. Meanwhile, I struggle with stairs.", AnnouncerType.Color),
                        new DialogueLineEntry("I should get a trampoline. Learn to jump like that. My doctor would be thrilled.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog is flying! Can we get him a pilot's license?", AnnouncerType.Color),
                        new DialogueLineEntry("I tried to do a flying leap once. Landed in a fountain. 10/10 would do again.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog's vertical leap could win a slam dunk contest.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder if the dog practices jumping at home. Like, over the couch.", AnnouncerType.Color),
                        new DialogueLineEntry("If that dog played volleyball, he'd never let a ball hit the ground.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog is part gazelle. Look at that grace!", AnnouncerType.Color),
                        new DialogueLineEntry("I need those jumping lessons. My long jump days are over, but maybe vertical?", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, that dog has been hitting the gym. Look at those quads!", AnnouncerType.Color),
                        new DialogueLineEntry("I tried to jump over a puddle once. Ended up in the puddle. Classic Buck.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog jumps like he's on a pogo stick. Boing, boing, boing!", AnnouncerType.Color),
                        new DialogueLineEntry("If the Olympics had a dog jumping event, this dog would get gold.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog is auditioning for a superhero movie. Captain Canine!", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has better jumping form than my karate kid impression.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
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
                        // Original 10 lines
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
                        // Additional Arthur lines (Mistakes)
                        new DialogueLineEntry("A run-out! The dog bypassed the obstacle entirely.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog is looking at the crowd. Focus is lost.", AnnouncerType.Main),
                        new DialogueLineEntry("A knocked bar. The dog clipped it with a hind leg.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler's timing was off. The dog committed too early.", AnnouncerType.Main),
                        new DialogueLineEntry("A wrong course fault. The dog took obstacle three instead of four.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog is distracted by something off-camera.", AnnouncerType.Main),
                        new DialogueLineEntry("A refusal at the weave poles. The dog stopped short.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler is trying to regain control. Time is ticking away.", AnnouncerType.Main),
                        new DialogueLineEntry("A fly-off from the teeter. The dog left before the board touched.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog is spinning. The handler's cues are unclear.", AnnouncerType.Main),
                        new DialogueLineEntry("A missed contact zone. The dog flew right over it.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog stopped to scratch. A charming but costly distraction.", AnnouncerType.Main),
                        new DialogueLineEntry("A refusal at the tunnel. The dog wouldn't enter.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler crossed the dog's path. Confusion ensued.", AnnouncerType.Main),
                        new DialogueLineEntry("A knocked bar on the triple spread. Devastating.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog is looking for treats instead of obstacles.", AnnouncerType.Main),
                        new DialogueLineEntry("A run-out at the jump. The dog went around it.", AnnouncerType.Main),
                        new DialogueLineEntry("The connection is broken. The handler is rebuilding.", AnnouncerType.Main),
                        new DialogueLineEntry("A costly error. The fault count is climbing.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog is hesitant. The confidence is gone.", AnnouncerType.Main),
                        new DialogueLineEntry("A wrong course fault. The dog is lost.", AnnouncerType.Main),
                        new DialogueLineEntry("The handler is making desperate gestures.", AnnouncerType.Main),
                        new DialogueLineEntry("A refusal. The dog simply refused to cooperate.", AnnouncerType.Main),
                        new DialogueLineEntry("The run is in shambles. Time to regroup.", AnnouncerType.Main),
                        new DialogueLineEntry("A missed contact. The fault is confirmed.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog is confused. The handler is frustrated.", AnnouncerType.Main),
                        new DialogueLineEntry("A knocked bar. The score is taking a hit.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog is distracted. The handler is trying to refocus.", AnnouncerType.Main),
                        new DialogueLineEntry("A run-out. The dog bypassed the obstacle.", AnnouncerType.Main),
                        new DialogueLineEntry("The run has fallen apart. A tough break.", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1.5f, 1f, 1f, 2f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        // Original 7 lines
                        new DialogueLineEntry("He's sniffing the ground! I think he found oil, Arthur. We're rich!", AnnouncerType.Color),
                        new DialogueLineEntry("The dog is looking at me. Stop looking at me! I don't have the jerky anymore!", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, what if the dog just speaks Spanish and doesn't understand the lady?", AnnouncerType.Color),
                        new DialogueLineEntry("He's making his own course. I respect that. A true artist doesn't follow lines.", AnnouncerType.Color),
                        new DialogueLineEntry("Is there a penalty if the dog just goes over to the crowd and asks for a hot dog?", AnnouncerType.Color),
                        new DialogueLineEntry("Well, that was a trainwreck. Should we go down there and help? I have a whistle.", AnnouncerType.Color),
                        new DialogueLineEntry("The dog is looking at the handler like she owes him money.", AnnouncerType.Color),
                        // Additional Buck lines (Mistakes)
                        new DialogueLineEntry("I've seen better focus from a squirrel with ADHD.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog just noped right out of that obstacle. I respect the honesty.", AnnouncerType.Color),
                        new DialogueLineEntry("If this were a test, that dog just failed with flying colors.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog is having an existential crisis. 'Why am I running?'", AnnouncerType.Color),
                        new DialogueLineEntry("That's me every Monday morning. Just completely lost and confused.", AnnouncerType.Color),
                        new DialogueLineEntry("I once forgot where I parked. This dog forgot where he's running. Similar energy.", AnnouncerType.Color),
                        new DialogueLineEntry("If dogs had Yelp reviews, that run would get one star. Would not recommend.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I think the dog is staging a protest. 'No more obstacles!'", AnnouncerType.Color),
                        new DialogueLineEntry("I've seen better direction from a tourist without a map.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog is definitely freelance running. No rules, no boundaries.", AnnouncerType.Color),
                        new DialogueLineEntry("If confusion were a sport, that dog would be the champion.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog just did an obstacle course of his own design. Interesting choices.", AnnouncerType.Color),
                        new DialogueLineEntry("That's the dog version of 'I'm not mad, I'm just disappointed.'", AnnouncerType.Color),
                        new DialogueLineEntry("I tried to follow that run and now I need a therapist.", AnnouncerType.Color),
                        new DialogueLineEntry("If the dog wrote a memoir, it would be titled 'Lost and Confused: My Journey.'", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog just realized he's not a cat. Explains a lot.", AnnouncerType.Color),
                        new DialogueLineEntry("That's me trying to assemble IKEA furniture. Just random parts going everywhere.", AnnouncerType.Color),
                        new DialogueLineEntry("If chaos were an art form, that dog would be Picasso.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog just invented a new sport. Chaotic Running. Patent pending.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I think the dog is having a mid-life crisis. 'What am I doing with my life?'", AnnouncerType.Color),
                        new DialogueLineEntry("That run was so bad, even the cat judging from the sidelines looked embarrassed.", AnnouncerType.Color),
                        new DialogueLineEntry("If that dog were a GPS, it would say 'Recalculating' every two seconds.", AnnouncerType.Color),
                        new DialogueLineEntry("I think the dog just realized the treat isn't at the finish line. Motivation lost.", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
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
                        // Original 10 lines
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
                        // Additional Arthur lines (Finish Line)
                        new DialogueLineEntry("A new personal best! What a run!", AnnouncerType.Main),
                        new DialogueLineEntry("The clock stops. An incredible time!", AnnouncerType.Main),
                        new DialogueLineEntry("The handler celebrates. They've earned it.", AnnouncerType.Main),
                        new DialogueLineEntry("A flawless finish. Nothing left on the table.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog crosses the line, tail wagging. A job well done.", AnnouncerType.Main),
                        new DialogueLineEntry("What a way to end the competition!", AnnouncerType.Main),
                        new DialogueLineEntry("A clean run under pressure. Championship material.", AnnouncerType.Main),
                        new DialogueLineEntry("The crowd erupts. They witnessed something special.", AnnouncerType.Main),
                        new DialogueLineEntry("That time will be hard to beat.", AnnouncerType.Main),
                        new DialogueLineEntry("A moment of pure joy. Handler and dog embrace.", AnnouncerType.Main),
                        new DialogueLineEntry("They've done it. A new course record!", AnnouncerType.Main),
                        new DialogueLineEntry("A photo finish. The timers confirm the result.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog knows. He's celebrating too.", AnnouncerType.Main),
                        new DialogueLineEntry("A textbook finish. Every obstacle executed perfectly.", AnnouncerType.Main),
                        new DialogueLineEntry("The handlers shake hands. Respect among competitors.", AnnouncerType.Main),
                        new DialogueLineEntry("What a tournament this has been!", AnnouncerType.Main),
                        new DialogueLineEntry("A fitting end to an incredible day of agility.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog takes a bow. A true champion.", AnnouncerType.Main),
                        new DialogueLineEntry("We've witnessed history today.", AnnouncerType.Main),
                        new DialogueLineEntry("A run that will be remembered for years to come.", AnnouncerType.Main),
                        new DialogueLineEntry("The final standings are in. What a competition!", AnnouncerType.Main),
                        new DialogueLineEntry("A clean sweep. Dominant performance.", AnnouncerType.Main),
                        new DialogueLineEntry("The crowd shows their appreciation. Well deserved.", AnnouncerType.Main),
                        new DialogueLineEntry("A masterclass from start to finish.", AnnouncerType.Main),
                        new DialogueLineEntry("That's how you close out a tournament.", AnnouncerType.Main),
                        new DialogueLineEntry("A legendary run. We are honored to have witnessed it.", AnnouncerType.Main),
                        new DialogueLineEntry("The dog is the star today. And rightly so.", AnnouncerType.Main),
                        new DialogueLineEntry("A beautiful ending to a beautiful competition.", AnnouncerType.Main),
                        new DialogueLineEntry("The team celebrates. They've achieved the impossible.", AnnouncerType.Main),
                        new DialogueLineEntry("Thank you, and goodnight from the agility arena!", AnnouncerType.Main),
                    },
                    weights = new List<float> { 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1.5f, 1.5f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1.5f, 1.5f, 1f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1.5f, 1.5f, 1f, 1f, 1.5f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1.5f, 1f, 1f }
                },
                buckPool = new DialoguePool
                {
                    sourceLines = new List<DialogueLineEntry>
                    {
                        // Original 7 lines
                        new DialogueLineEntry("He did it! Does he get a giant novelty check? Dogs love chewing on cardboard.", AnnouncerType.Color),
                        new DialogueLineEntry("I'm exhausted just watching that. Arthur, carry me to the car.", AnnouncerType.Color),
                        new DialogueLineEntry("You know, at the end of the day, it's just a wolf running over a piece of wood.", AnnouncerType.Color),
                        new DialogueLineEntry("I wonder what that dog's going to do to celebrate. Probably drink out of the toilet.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, tell the truth: Are any of these dogs wearing a wire? It feels rigged.", AnnouncerType.Color),
                        new DialogueLineEntry("I'm going down to the floor to see if I can adopt the loser. I like an underdog.", AnnouncerType.Color),
                        new DialogueLineEntry("Goodnight everyone! Remember to spay and neuter your commentators!", AnnouncerType.Color),
                        // Additional Buck lines (Finish Line)
                        new DialogueLineEntry("That dog just won! Do they get a trophy? Please tell me it's shaped like a bone.", AnnouncerType.Color),
                        new DialogueLineEntry("I'm crying, Arthur. Those are happy tears. And maybe a little allergies.", AnnouncerType.Color),
                        new DialogueLineEntry("If I had a dog like that, I'd never need a gym membership. Just play fetch!", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, that dog just made more money than I made all year. Can I be a dog?", AnnouncerType.Color),
                        new DialogueLineEntry("I need to go lie down. That was too much excitement for one human.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog is going to have the best dinner tonight. Steak and everything!", AnnouncerType.Color),
                        new DialogueLineEntry("I just witnessed perfection. And I don't say that lightly. I once ate a perfect pizza.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, can we get that dog a cape? He's a superhero!", AnnouncerType.Color),
                        new DialogueLineEntry("I think I'm going to adopt a dog now. This is all your fault, agility.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has more followers than me. And I have a great Instagram.", AnnouncerType.Color),
                        new DialogueLineEntry("I want to party with that dog. He knows how to celebrate!", AnnouncerType.Color),
                        new DialogueLineEntry("If that dog ever runs for office, he has my vote. Unanimous!", AnnouncerType.Color),
                        new DialogueLineEntry("I'm going to tell everyone I was here. Nobody will believe me, but that's okay.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog deserves a movie deal. 'The Fast and the Furriest.' Coming to theaters!", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, I'm going to name my firstborn after that dog. Don't judge me.", AnnouncerType.Color),
                        new DialogueLineEntry("I just realized something. That dog is better at life than I am.", AnnouncerType.Color),
                        new DialogueLineEntry("If the Olympics need a new sport, this is it. Agility. Let's make it happen!", AnnouncerType.Color),
                        new DialogueLineEntry("I need a commemorative T-shirt. 'I watched [Dog's Name] win the championship.'", AnnouncerType.Color),
                        new DialogueLineEntry("That dog just inspired me to get off the couch. Maybe tomorrow.", AnnouncerType.Color),
                        new DialogueLineEntry("I'm going to have dreams about that run. Good dreams. Happy dreams.", AnnouncerType.Color),
                        new DialogueLineEntry("Arthur, we need to have a watch party for the next tournament. I'll bring the jerky.", AnnouncerType.Color),
                        new DialogueLineEntry("That dog has changed my life. I'm a different person now.", AnnouncerType.Color),
                        new DialogueLineEntry("I want to send that dog a fruit basket. Does that work for dogs?", AnnouncerType.Color),
                        new DialogueLineEntry("I think I just found my new favorite sport. Move over, bowling!", AnnouncerType.Color),
                    },
                    weights = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }
                }
            };
        }
        
        #endregion
    }
}
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Services;

namespace AgilityDogs.Demo
{
    [Serializable]
    public class DemoSegment
    {
        public string segmentTitle;
        public string subtitle;
        [TextArea(2, 4)]
        public string description;
        public List<DemoDialogueLine> lines = new List<DemoDialogueLine>();
        public float segmentDuration = 20f;
        public bool showBranding;
        public string brandingTitle;
        [TextArea(1, 2)]
        public string brandingSubtitle;
    }

    [Serializable]
    public class DemoDialogueLine
    {
        public string speakerName;
        public CommentatorType speakerType;
        [TextArea(2, 6)]
        public string text;
        public float duration = 4f;
        public float delayBefore = 0.5f;
        public string emotion;
    }

    public static class DemoDialogueContent
    {
        public static List<DemoSegment> BuildSegments()
        {
            return new List<DemoSegment>
            {
                BuildOpeningSegment(),
                BuildDogSelectionSegment(),
                BuildCompetitionSegment(),
                BuildTrainingSegment(),
                BuildClosingSegment()
            };
        }

        static DemoSegment BuildOpeningSegment()
        {
            return new DemoSegment
            {
                segmentTitle = "opening",
                subtitle = "",
                description = "",
                showBranding = true,
                brandingTitle = "AGILITY DOGS",
                brandingSubtitle = "The Journey from Puppy to Champion",
                segmentDuration = 22f,
                lines = new List<DemoDialogueLine>
                {
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Good evening, everyone! Arthur Mitchell here, welcoming you to a very special look inside the world of competitive dog agility.",
                        duration = 6f,
                        delayBefore = 3f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "And I'm Buck Dawson, former champion handler. Arthur, I've been looking forward to this all week.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Tonight, we're going to take you behind the scenes. From the moment a handler picks their puppy, through grueling training sessions, all the way to the bright lights of competition.",
                        duration = 7f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "And trust me, what you're about to see will change how you think about this sport forever.",
                        duration = 5f
                    }
                }
            };
        }

        static DemoSegment BuildDogSelectionSegment()
        {
            return new DemoSegment
            {
                segmentTitle = "Dog Selection & Grooming",
                subtitle = "Choosing Your Champion",
                description = "Where every journey begins",
                segmentDuration = 35f,
                lines = new List<DemoDialogueLine>
                {
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Let's start where every great team begins - the selection. This is where handler and dog first lock eyes, and something magical happens.",
                        duration = 6f,
                        delayBefore = 1f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "I remember choosing my first dog, Arthur. A scrappy little Border Collie named Blitz. Didn't look like much, but that dog had fire in her eyes.",
                        duration = 6f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Look at this - the handler is sizing up each breed. Speed, agility, trainability - every attribute matters.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "See how the Corgi's ears perk up? That's intelligence right there. People underestimate them because of those short legs, but Corgis are some of the smartest dogs on the course.",
                        duration = 6f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "And now the grooming begins. This isn't just about looks - grooming builds the physical bond between handler and dog. Trust starts here.",
                        duration = 6f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "Notice how calm the dog is? That's the sign of a great partnership forming. A nervous dog won't perform under pressure.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "The Golden Retriever gets a brush, the Beagle gets a check-up. Every breed has different needs, and a good handler knows them all.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "Arthur, I've seen handlers skip this step and pay for it later. The grooming table is where championships are won or lost, I'm telling you.",
                        duration = 6f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Strong words from a former champion! But I think the footage speaks for itself. Look at that connection.",
                        duration = 5f
                    }
                }
            };
        }

        static DemoSegment BuildCompetitionSegment()
        {
            return new DemoSegment
            {
                segmentTitle = "Competition",
                subtitle = "Under the Bright Lights",
                description = "Where heroes are made",
                segmentDuration = 45f,
                lines = new List<DemoDialogueLine>
                {
                    new DemoDialogueLine
                    {
                        speakerName = "PA Announcer",
                        speakerType = CommentatorType.PA,
                        text = "Ladies and gentlemen, welcome to the County Fair Agility Championship! Handlers, to your marks!",
                        duration = 5f,
                        delayBefore = 1f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Here we go! The moment we've all been waiting for! The crowd is on their feet!",
                        duration = 4f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "Look at the handler's stance - focused, balanced, ready to move. That's textbook form.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "THEY'RE OFF! The dog explodes out of the gate! First jump - CLEAN! Beautiful arc over the bar!",
                        duration = 5f,
                        emotion = "excited"
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "That acceleration is remarkable. The handler's body language is perfect - clear directionals, no hesitation.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Into the tunnel! The dog disappears... and BURSTS out the other side! Flawless entry and exit!",
                        duration = 5f,
                        emotion = "excited"
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "Now here's where it gets tricky - the weave poles. This separates the contenders from the pretenders.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "The entry is PERFECT! Left side of the first pole, just like Coach Sarah teaches! The rhythm is incredible!",
                        duration = 5f,
                        emotion = "excited"
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "Speed AND accuracy through the weaves. That's not luck, Arthur - that's thousands of hours of practice.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "A-Frame! The dog flies up... and DOWN! Contact zone HIT! The judge gives the green signal!",
                        duration = 5f,
                        emotion = "excited"
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "Oh, I love watching a clean A-frame. The power, the precision - that's what this sport is all about, Arthur.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Final approach! The tire jump - they clear it with INCHES to spare! And it's a SPRINT to the finish line!",
                        duration = 5f,
                        emotion = "excited"
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "The handler is RUNNING alongside, keeping pace. The trust between them is absolute.",
                        duration = 4f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "THEY CROSS THE LINE! LOOK AT THAT TIME! Zero faults, a personal best, and THIS CROWD HAS GONE ABSOLUTELY WILD!",
                        duration = 6f,
                        emotion = "ecstatic"
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "That, my friends, is what a championship run looks like. Textbook. Poetry in motion. I'm getting chills over here.",
                        duration = 6f
                    }
                }
            };
        }

        static DemoSegment BuildTrainingSegment()
        {
            return new DemoSegment
            {
                segmentTitle = "Training & Practice",
                subtitle = "Champions Are Made Here",
                description = "Where the magic happens",
                segmentDuration = 30f,
                lines = new List<DemoDialogueLine>
                {
                    new DemoDialogueLine
                    {
                        speakerName = "Coach Sarah",
                        speakerType = CommentatorType.Color,
                        text = "Alright, let's slow things down and talk about what really makes a champion. Training. Consistent, patient, dedicated training.",
                        duration = 6f,
                        delayBefore = 1f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Coach Sarah Chen, everyone. Twenty years of training champions, and she's going to walk us through the fundamentals.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Coach Sarah",
                        speakerType = CommentatorType.Color,
                        text = "Every session starts with bonding. Your dog has to trust you completely before they'll perform at their peak. Watch how the handler rewards even the smallest progress.",
                        duration = 6f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "Coach is right. I've seen naturally talented teams fail because they skipped the basics. You can't rush trust.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Coach Sarah",
                        speakerType = CommentatorType.Color,
                        text = "Now watch this - the handler is working on weave pole entries. They start slow, building muscle memory. Speed comes later, naturally.",
                        duration = 6f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "Look at the patience! The dog misses the entry, but the handler doesn't get frustrated. They reset and try again. That's the mark of a great handler.",
                        duration = 6f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "I tell every young handler the same thing - your dog is reading your emotions. If you're frustrated, they feel it. Stay calm, stay positive.",
                        duration = 6f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Coach Sarah",
                        speakerType = CommentatorType.Color,
                        text = "Perfect! See that? The entry was clean this time. That's the power of positive reinforcement. This team is going places.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "And that's what training mode in Agility Dogs is all about. No pressure, no clock, just you and your partner getting better together.",
                        duration = 6f
                    }
                }
            };
        }

        static DemoSegment BuildClosingSegment()
        {
            return new DemoSegment
            {
                segmentTitle = "closing",
                subtitle = "",
                description = "",
                showBranding = true,
                brandingTitle = "AGILITY DOGS",
                brandingSubtitle = "Press Any Key to Start Your Journey",
                segmentDuration = 18f,
                lines = new List<DemoDialogueLine>
                {
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "From that first glance across the kennel, to the roar of the crowd at the finish line - this is agility.",
                        duration = 6f,
                        delayBefore = 1f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Buck Dawson",
                        speakerType = CommentatorType.Color,
                        text = "And that's just a taste of what awaits you. The full experience? You'll have to see it for yourself.",
                        duration = 5f
                    },
                    new DemoDialogueLine
                    {
                        speakerName = "Arthur Mitchell",
                        speakerType = CommentatorType.Main,
                        text = "This has been Arthur Mitchell alongside Buck Dawson, saying - your champion is waiting. See you on the course!",
                        duration = 6f
                    }
                }
            };
        }
    }
}

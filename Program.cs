using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Functionality;

namespace Flashcards
{
    class Program
    {
        public static string titleText = "Flashcards                              Version 2.0";
        public static Functionality.Text text = new Functionality.Text();
        public static Functionality.Options ops = new Functionality.Options();
        public static Functionality.JSONSaving jsSave = new Functionality.JSONSaving();
        static string JSONFilePath = (Directory.GetCurrentDirectory() + @"\saveData.json");
        public static List<CardDeck> decks = new List<CardDeck>();
        public static Random rand = new Random();

        public static string answer = "";
        public static int newDecks = 0;
        public static bool leaveDeckView = false;
     
        static void Main(string[] args)
        {
            Load();

            Menu();

            Exit();


            void Menu()
            {
                bool inMenu = true;
                while (inMenu)
                {
                    Dictionary<string, Action> actionDict = new Dictionary<string, Action>();
                    actionDict.Add("View Current Decks", ViewCurrentDecks);
                    actionDict.Add("New Deck", NewDeck);
                    ops.ActionOptions(titleText, actionDict, ref inMenu).Invoke();

                    void ViewCurrentDecks()
                    {
                        Console.Clear();

                        if (decks.Count <= 0)
                        {
                            Console.WriteLine("You need to make a deck!\n");
                            text.PressAnyKey();
                        }
                        else
                        {
                            ChooseADeck(decks);
                        }

                        void ChooseADeck(List<CardDeck> decks)
                        {
                            bool choosing = true;
                            while (choosing)
                            {
                                if (decks.Count <= 0)
                                {
                                    choosing = false;
                                }
                                else
                                {
                                    // Return deck names
                                    List<string> titles = new List<string>();
                                    foreach (CardDeck deck in decks)
                                    {
                                        titles.Add(deck.title);
                                    }

                                    int index = ops.WriteOptions_GetValidResponse("Choose a deck", titles, true);
                                    if (index == ops.exitValue)
                                    {
                                        choosing = false;
                                    }
                                    else
                                    {
                                        DeckOptions(decks[index]);
                                    }
                                }
                            }
                            void DeckOptions(CardDeck deckChosen)
                            {
                                bool inOptions = true;
                                while (inOptions)
                                {
                                    Dictionary<string, Action> actionDict2 = new Dictionary<string, Action>();
                                    actionDict2.Add("Quiz", Quiz);
                                    actionDict2.Add("Add Card", AddCard);
                                    actionDict2.Add("Rename", Rename);
                                    actionDict2.Add("DELETE DECK", Delete);
                                    ops.ActionOptions($"What to do with '{deckChosen.title}'?", actionDict2, ref inOptions).Invoke();

                                    void Quiz()
                                    {
                                        bool quizMenu = true;
                                        while (quizMenu)
                                        {
                                            if(deckChosen.flashcards.Count <= 0)
                                            {
                                                quizMenu = false;
                                            } 
                                            else
                                            {
                                                Dictionary<string, Action> actOpts = new Dictionary<string, Action>();
                                                actOpts.Add("Quiz in order", InOrder);
                                                actOpts.Add("Shuffle Deck", Shuffled);
                                                ops.ActionOptions("Quiz", actOpts, ref quizMenu).Invoke();

                                                void InOrder()
                                                {
                                                    QuizDeck(deckChosen.flashcards);
                                                }
                                                void Shuffled()
                                                {
                                                    List<Flashcard> shuffledCards = deckChosen.flashcards.OrderBy(_ => rand.Next()).ToList();
                                                    QuizDeck(shuffledCards);
                                                }

                                                void QuizDeck(List<Flashcard> cards)
                                                {
                                                    Console.Clear();
                                                    int cardCounter = 0;
                                                    bool quizzing = true;
                                                    // While card count is within limits
                                                    while ((cardCounter >= 0 && cardCounter < cards.Count) && quizzing)
                                                    {
                                                        SeeCard(cards[cardCounter]);
                                                    }

                                                    void SeeCard(Flashcard flashcard)
                                                    {
                                                        string adjustedIndex = (cardCounter + 1).ToString();

                                                        string markQuestion = "Mark Question";
                                                        if (flashcard.currentlyMarked == true)
                                                        {
                                                            markQuestion = "Unmark Question";
                                                        }

                                                        string cardDisplay = $"Deck: '{deckChosen.title}'     (Card: {adjustedIndex}/{cards.Count})\n\n";
                                                        Dictionary<string, Action> cardOpts = new Dictionary<string, Action>();
                                                        cardOpts.Add("See Answer", SeeAnswer);
                                                        cardOpts.Add("Next Card", NextCard);
                                                        cardOpts.Add("Previous Card", PreviousCard);
                                                        cardOpts.Add("Edit", Edit);
                                                        cardOpts.Add(markQuestion, MarkCard);
                                                        cardOpts.Add("DELETE", Delete);
                                                        ops.ActionOptions((cardDisplay + flashcard.question), cardOpts, ref quizzing).Invoke();


                                                        void SeeAnswer()
                                                        {
                                                            Console.Clear();
                                                            Console.WriteLine($"{cardDisplay}{flashcard.question}\n\nAnswer: {flashcard.answer}\n\n\n");
                                                            text.PressAnyKey();
                                                        }
                                                        void NextCard()
                                                        {
                                                            cardCounter++;
                                                        }
                                                        void PreviousCard()
                                                        {
                                                            cardCounter--;
                                                        }
                                                        void MarkCard()
                                                        {
                                                            if (flashcard.currentlyMarked == false)
                                                            {
                                                                flashcard.currentlyMarked = true;
                                                            }
                                                            else
                                                            {
                                                                flashcard.currentlyMarked = false;
                                                            }
                                                        }
                                                        void Edit()
                                                        {
                                                            bool editingCard = true;
                                                            while(editingCard)
                                                            {
                                                                string display = $"Question: {flashcard.question}\nAnswer: {flashcard.answer}\n\nWhat would you like to edit?";
                                                                Dictionary<string, Action> cardOpts1 = new Dictionary<string, Action>();
                                                                cardOpts1.Add("Question", EditQuestion);
                                                                cardOpts1.Add("Answer", EditAnswer);
                                                                ops.ActionOptions(display, cardOpts1, ref editingCard).Invoke();
                                                            }

                                                            void EditQuestion()
                                                            {
                                                                Console.WriteLine($"Change '{flashcard.question}' to ?");
                                                                flashcard.question = Console.ReadLine();
                                                                Console.Clear();
                                                                Save();
                                                            }

                                                            void EditAnswer()
                                                            {
                                                                Console.WriteLine($"Change '{flashcard.answer}' to ?");
                                                                flashcard.answer = Console.ReadLine();
                                                                Console.Clear();
                                                                Save();
                                                            }
                                                        }
                                                        void Delete()
                                                        {
                                                            deckChosen.flashcards.Remove(flashcard);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    void AddCard()
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Write the Question\n");
                                        string question = Console.ReadLine();

                                        Console.Clear();
                                        Console.WriteLine("Write the Answer\n");
                                        string answer = Console.ReadLine();

                                        deckChosen.flashcards.Add(new Flashcard(question, answer));

                                        Console.Clear();

                                        Save();
                                    }

                                    void Rename()
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Rename '" + deckChosen.title + "' here");
                                        string answer = Console.ReadLine();


                                        bool confirmed = text.AskToConfirm($"Is {answer} correct?");
                                        if (confirmed)
                                        {
                                            deckChosen.title = answer;
                                            Save();
                                        }
                                    }

                                    void Delete()
                                    {
                                        bool confirm = text.AskToConfirm($"Delete '{deckChosen.title}'");
                                        if (confirm)
                                        {
                                            Console.Clear();
                                            Console.WriteLine(deckChosen.title + " Deleted!");
                                            decks.Remove(deckChosen);
                                            text.PressAnyKey();
                                            inOptions = false;
                                            Save();
                                        }

                                    }
                                }

                            }
                        }
                    }
                    void NewDeck()
                    {
                        bool creatingDeck = true;
                        while (creatingDeck)
                        {
                            Console.WriteLine("Write New Deck Name:\n");
                            string deckname = Console.ReadLine();
                            Console.Clear();
                            if (string.IsNullOrWhiteSpace(deckname))
                            {
                                Console.WriteLine("NAME CANNOT BE BLANK!\n");
                                text.PressAnyKey();
                            }
                            else
                            {
                                Console.Clear();
                                decks.Add(new CardDeck(deckname, new List<Flashcard>()));
                                creatingDeck = false;
                            }
                        }
                    }
                }
            }
            void Exit()
            {
                Environment.Exit(0);
            }
        }
        static void AddDecks()
        {
            List<Flashcard> WordKnowledgeCards = new List<Flashcard>();
            WordKnowledgeCards.Add(new Flashcard("Remuneration", "Payment for work done."));
            WordKnowledgeCards.Add(new Flashcard("Reprehensible", "Deserving censure or condemnation."));
            WordKnowledgeCards.Add(new Flashcard("Sagacious", "Having or showing keen mental discernment and good judgment; shrewd."));
            WordKnowledgeCards.Add(new Flashcard("Spurious", "Not being what it purports to be; false or fake."));
            WordKnowledgeCards.Add(new Flashcard("Nuance", "A subtle difference in or shade of meaning, expression, or sound."));
            WordKnowledgeCards.Add(new Flashcard("Pernicious", "Having a harmful effect, especially in a gradual or subtle way."));
            WordKnowledgeCards.Add(new Flashcard("Prolific", "Present in large numbers or quantities; plentiful."));
            WordKnowledgeCards.Add(new Flashcard("Querulous", "Complaining in a petulant or whining manner."));
            WordKnowledgeCards.Add(new Flashcard("Rancorous", "Characterized by bitterness or resentment."));
            WordKnowledgeCards.Add(new Flashcard("Sycophant", "A person who acts obsequiously toward someone important in order to gain advantage."));
            WordKnowledgeCards.Add(new Flashcard("Trepidation", "A feeling of fear or agitation about something that may happen."));
            WordKnowledgeCards.Add(new Flashcard("Ubiquitous", "Present, appearing, or found everywhere."));
            WordKnowledgeCards.Add(new Flashcard("Venerable", "Accorded a great deal of respect, especially because of age, wisdom, or character."));
            WordKnowledgeCards.Add(new Flashcard("Vindicate", "Clear (someone) of blame or suspicion."));
            WordKnowledgeCards.Add(new Flashcard("Bilk", "Obtain or withhold money from (someone) by deceit or without justification; cheat or defraud."));
            WordKnowledgeCards.Add(new Flashcard("Engender", "Cause or give rise to (a feeling, situation, or condition)."));
            WordKnowledgeCards.Add(new Flashcard("Knotty", "Extremely difficult or intricate."));
            WordKnowledgeCards.Add(new Flashcard("Nuance", "A subtle difference in or shade of meaning, expression, or sound."));
            WordKnowledgeCards.Add(new Flashcard("Abasement", "Humiliation or degradation."));
            WordKnowledgeCards.Add(new Flashcard("Harangue", "A lengthy and aggressive speech."));
            WordKnowledgeCards.Add(new Flashcard("Plantiff", "A person who brings a case against another in a court of law."));
            WordKnowledgeCards.Add(new Flashcard("Abrogate", "Repeal or do away with (a law, right, or formal agreement)."));
            WordKnowledgeCards.Add(new Flashcard("Plaudit", "An expression of praise or approval."));
            WordKnowledgeCards.Add(new Flashcard("Ensconce", "Establish or settle (someone) in a comfortable, safe, or secret place."));
            WordKnowledgeCards.Add(new Flashcard("Obdurate", "Stubbornly refusing to change one's opinion or course of action."));
            WordKnowledgeCards.Add(new Flashcard("Reprieve", "Cancel or postpone the punishment of (someone, especially someone condemned to death)."));
            WordKnowledgeCards.Add(new Flashcard("Credulous", "Having or showing too great a readiness to believe things."));
            WordKnowledgeCards.Add(new Flashcard("Haughtiness", "The appearance or quality of being arrogantly superior and disdainful."));
            WordKnowledgeCards.Add(new Flashcard("Lachrymose", "Tearful or given to weeping."));
            WordKnowledgeCards.Add(new Flashcard("Obfuscate", "Render obscure, unclear, or unintelligible."));
            WordKnowledgeCards.Add(new Flashcard("Tedious", "Too long, slow, or dull; tiresome or monotonous."));
            WordKnowledgeCards.Add(new Flashcard("Abstemious", "Not self-indulgent, especially when eating and drinking."));
            WordKnowledgeCards.Add(new Flashcard("Crepescular", "Of, resembling, or relating to twilight."));
            WordKnowledgeCards.Add(new Flashcard("Pliable", "Easily bent; flexible."));
            WordKnowledgeCards.Add(new Flashcard("Abstruse", "Difficult to understand; obscure."));
            WordKnowledgeCards.Add(new Flashcard("Laconic", "Using very few words."));
            WordKnowledgeCards.Add(new Flashcard("Oblique", "Neither parallel nor at a right angle to a specified or implied line; slanting."));
            WordKnowledgeCards.Add(new Flashcard("Lament", "A passionate expression of grief or sorrow."));
            WordKnowledgeCards.Add(new Flashcard("Tenative", "Not certain or fixed; provisional."));
            WordKnowledgeCards.Add(new Flashcard("Acquiesce", "Accept something reluctantly but without protest."));
            WordKnowledgeCards.Add(new Flashcard("Epicure", "A person who takes particular pleasure in fine food and drink."));
            WordKnowledgeCards.Add(new Flashcard("Tenous", "Very weak or slight."));
            WordKnowledgeCards.Add(new Flashcard("Epistolary", "Of, relating to, or denoting the writing of letters."));
            WordKnowledgeCards.Add(new Flashcard("Languid", "Displaying or having a disinclination for physical exertion or effort; slow and relaxed."));
            WordKnowledgeCards.Add(new Flashcard("Poised", "Having a composed and self-assured manner."));
            WordKnowledgeCards.Add(new Flashcard("Hidebound", "Unwilling or unable to change because of tradition or convention."));
            WordKnowledgeCards.Add(new Flashcard("Languish", "Lose or lack vitality; grow weak or feeble."));
            WordKnowledgeCards.Add(new Flashcard("Obsequious", "Obedient or attentive to an excessive or servile degree."));
            WordKnowledgeCards.Add(new Flashcard("Polemical", "Of or involving strongly critical, controversial, or disputatious writing or speech."));
            WordKnowledgeCards.Add(new Flashcard("Equivocate", "Use ambiguous language so as to conceal the truth or avoid committing oneself."));
            WordKnowledgeCards.Add(new Flashcard("Ponderous", "Slow and clumsy because of great weight."));
            WordKnowledgeCards.Add(new Flashcard("Adroit", "Clever or skillful in using the hands or mind."));
            WordKnowledgeCards.Add(new Flashcard("Largess", "Generosity in bestowing money or gifts upon others."));
            WordKnowledgeCards.Add(new Flashcard("Pontificate", "Express one's opinions in a way considered annoyingly pompous and dogmatic."));
            WordKnowledgeCards.Add(new Flashcard("Reticent", "Not revealing one's thoughts or feelings readily."));
            WordKnowledgeCards.Add(new Flashcard("Histrionic", "Overly theatrical or melodramatic in character or style."));
            WordKnowledgeCards.Add(new Flashcard("Obstreperous", "Noisy and difficult to control."));
            WordKnowledgeCards.Add(new Flashcard("Portend", "Be a sign or warning that (something, especially something momentous or calamitous) is likely to happen."));
            WordKnowledgeCards.Add(new Flashcard("Deference", "Humble submission and respect."));
            WordKnowledgeCards.Add(new Flashcard("Hoary", "Grayish white."));
            WordKnowledgeCards.Add(new Flashcard("Brusque", "Blunt in manner or speech often to the point of ungracious harshness."));
            WordKnowledgeCards.Add(new Flashcard("Defoliate", "To deprive of leaves."));
            WordKnowledgeCards.Add(new Flashcard("Espouse", "To take up and support as a cause."));
            WordKnowledgeCards.Add(new Flashcard("Obviate", "To anticipate and prevent (as a situation) or make unnecessary (as an action)."));
            WordKnowledgeCards.Add(new Flashcard("Torpid", "Having lost motion or the power of exertion or feeling."));
            WordKnowledgeCards.Add(new Flashcard("Bulwark", "A solid wall-like structure raised for defense."));
            WordKnowledgeCards.Add(new Flashcard("Odious", "Arousing or deserving hatred or repugnance."));
            WordKnowledgeCards.Add(new Flashcard("Posterity", "All future generations."));
            WordKnowledgeCards.Add(new Flashcard("Rife", "Widespread, common."));
            WordKnowledgeCards.Add(new Flashcard("Torpor", "A state of mental and motor inactivity with partial or total insensibility."));
            WordKnowledgeCards.Add(new Flashcard("Affable", "Being pleasant and at ease in talking to others."));
            WordKnowledgeCards.Add(new Flashcard("Alacrity", "Promptness in response; cheerful readiness."));
            WordKnowledgeCards.Add(new Flashcard("Burgeon", "To send forth new growth (as buds or branches)."));
            WordKnowledgeCards.Add(new Flashcard("Deleterious", "Harmful often in a subtle or unexpected way."));
            WordKnowledgeCards.Add(new Flashcard("Euphemism", "The substitution of an agreeable or inoffensive expression for one that may offend or suggest something unpleasant."));
            WordKnowledgeCards.Add(new Flashcard("Tractable", "Easily handled or managed."));
            WordKnowledgeCards.Add(new Flashcard("Euphony", "A harmonious succession of words having a pleasing sound."));
            WordKnowledgeCards.Add(new Flashcard("Iconoclast", "A person who attacks settled beliefs or institutions."));
            WordKnowledgeCards.Add(new Flashcard("Levity", "Excessive or unseemly frivolity."));
            WordKnowledgeCards.Add(new Flashcard("Buttress", "Strengthen or support"));
            WordKnowledgeCards.Add(new Flashcard("Idiosyncrasy", "A peculiarity of constitution or temperament; an individualizing characteristic or quality."));
            WordKnowledgeCards.Add(new Flashcard("Allay", "To subdue or reduce in intensity or severity."));
            WordKnowledgeCards.Add(new Flashcard("Demur", "To take exception; object."));
            WordKnowledgeCards.Add(new Flashcard("Transgression", "An act, process, or instance of transgressing."));
            WordKnowledgeCards.Add(new Flashcard("Omniscient", "Having infinite awareness, understanding, and insight."));
            WordKnowledgeCards.Add(new Flashcard("Transient", "Passing especially quickly into and out of existence."));
            WordKnowledgeCards.Add(new Flashcard("Onerous", "Involving, imposing, or constituting a burden."));
            WordKnowledgeCards.Add(new Flashcard("Altruism", "Unselfish regard for or devotion to the welfare of others."));
            WordKnowledgeCards.Add(new Flashcard("Deplore", "To feel or express grief for."));
            WordKnowledgeCards.Add(new Flashcard("Exculpate", "To clear from alleged fault or guilt."));
            WordKnowledgeCards.Add(new Flashcard("callow", "Lacking adult sophistication."));
            WordKnowledgeCards.Add(new Flashcard("Execrable", "Deserving to be execrated; detestable."));
            WordKnowledgeCards.Add(new Flashcard("Lofty", "Elevated in character and spirit."));
            WordKnowledgeCards.Add(new Flashcard("Candid", "Marked by honest sincere expression."));
            WordKnowledgeCards.Add(new Flashcard("Exegesis", "Explanation or critical interpretation of a text."));
            WordKnowledgeCards.Add(new Flashcard("Salacious", "Lustful or lecherous."));
            WordKnowledgeCards.Add(new Flashcard("Trite", "Hackneyed or boring from much use; not fresh or original."));
            WordKnowledgeCards.Add(new Flashcard("Deride", "To laugh at or insult contemptuously."));
            WordKnowledgeCards.Add(new Flashcard("Loquacious", "Full of excessive talk."));
            WordKnowledgeCards.Add(new Flashcard("Ordain", "To invest officially with ministerial or priestly authority."));
            WordKnowledgeCards.Add(new Flashcard("Sallow", "Of a sickly yellowish hue or complexion."));
            WordKnowledgeCards.Add(new Flashcard("Clemency", "Disposition to be merciful and especially to moderate the severity of punishment due."));
            WordKnowledgeCards.Add(new Flashcard("Flout", "To treat with contemptuous disregard."));
            WordKnowledgeCards.Add(new Flashcard("Paucity", "Smallness of number; fewness."));
            WordKnowledgeCards.Add(new Flashcard("Protean", "Displaying great diversity or variety."));
            WordKnowledgeCards.Add(new Flashcard("Venal", "Capable of being bought or obtained for money or other valuable consideration."));
            WordKnowledgeCards.Add(new Flashcard("Ingrate", "An ungrateful person."));
            WordKnowledgeCards.Add(new Flashcard("Sophoric", "Inducing or tending to induce sleep."));
            WordKnowledgeCards.Add(new Flashcard("Inimical", "Being adverse often by reason of hostility or malevolence."));
            WordKnowledgeCards.Add(new Flashcard("Mercurial", "Characterized by rapid and unpredictable changeableness of mood."));
            WordKnowledgeCards.Add(new Flashcard("Venerate", "To regard with reverential respect or with admiring deference."));
            WordKnowledgeCards.Add(new Flashcard("Artifice", "Clever or artful skill; ingenuity."));
            WordKnowledgeCards.Add(new Flashcard("Provincial", "Limited in outlook; narrow."));
            WordKnowledgeCards.Add(new Flashcard("Specious", "Having a false look of truth or genuineness."));
            WordKnowledgeCards.Add(new Flashcard("Venial", "Easily excused or forgiven."));
            WordKnowledgeCards.Add(new Flashcard("Cantankerous", "Difficult or irritating to deal with."));
            WordKnowledgeCards.Add(new Flashcard("Impecunious", "Having very little or no money."));
            WordKnowledgeCards.Add(new Flashcard("Ornate", "Marked by elaborate rhetoric or florid style."));
            WordKnowledgeCards.Add(new Flashcard("Salubrious", "Favorable to or promoting health or well-being."));
            WordKnowledgeCards.Add(new Flashcard("Truant", "One who shirks duty."));
            WordKnowledgeCards.Add(new Flashcard("Ambulatory", "Of, relating to, or adapted to walking."));
            WordKnowledgeCards.Add(new Flashcard("Capacious", "Capable of containing a large quantity; spacious or roomy."));
            WordKnowledgeCards.Add(new Flashcard("Precipitous", "Done in a hurry"));
            WordKnowledgeCards.Add(new Flashcard("Salutary", "Producing a beneficial effect; remedial."));
            WordKnowledgeCards.Add(new Flashcard("Carping", "Marked by or inclined to querulous and often perverse criticism."));
            WordKnowledgeCards.Add(new Flashcard("Lummox", "A clumsy person."));
            WordKnowledgeCards.Add(new Flashcard("Ostentatious", "Marked by or fond of conspicuous or vainglorious and sometimes pretentious display."));
            WordKnowledgeCards.Add(new Flashcard("Precocious", "Exceptionally early in development or occurrence."));
            WordKnowledgeCards.Add(new Flashcard("Despondent", "Feeling or showing extreme discouragement, dejection, or depression."));
            WordKnowledgeCards.Add(new Flashcard("Expatriate", "To withdraw (oneself) from residence in or allegiance to one's native country."));
            WordKnowledgeCards.Add(new Flashcard("Inadvertent", "Not focusing the mind on a matter; inattentive."));
            WordKnowledgeCards.Add(new Flashcard("Luscious", "Having a delicious taste or smell."));
            WordKnowledgeCards.Add(new Flashcard("Sanguinary", "Bloodthirsty."));
            WordKnowledgeCards.Add(new Flashcard("Tyro", "A beginner in learning."));
            WordKnowledgeCards.Add(new Flashcard("Castigate", "To subject to severe punishment, reproof, or criticism."));
            WordKnowledgeCards.Add(new Flashcard("Amorphous", "Having no definite form; shapeless."));
            WordKnowledgeCards.Add(new Flashcard("Preeminent", "Having paramount rank, dignity, or importance."));
            WordKnowledgeCards.Add(new Flashcard("Sardonic", "Scornfully or cynically mocking."));
            WordKnowledgeCards.Add(new Flashcard("Unalloyed", "Pure, unmixed."));
            WordKnowledgeCards.Add(new Flashcard("Analgesic", "A remedy that relieves or allays pain."));
            WordKnowledgeCards.Add(new Flashcard("Circumspect", "Careful to consider all circumstances and possible consequences."));
            WordKnowledgeCards.Add(new Flashcard("Inexpedient", "Not tending to promote the purpose."));
            WordKnowledgeCards.Add(new Flashcard("Proscribe", "To publish the name of as condemned to death with the property of the condemned forfeited to the state."));
            WordKnowledgeCards.Add(new Flashcard("Vapid", "Lacking liveliness, tang, briskness, or force."));
            WordKnowledgeCards.Add(new Flashcard("Prosaic", "Dull, unimaginative."));
            WordKnowledgeCards.Add(new Flashcard("Discursive", "Moving from topic to topic without order."));
            WordKnowledgeCards.Add(new Flashcard("Flippant", "Marked by disrespectful levity or casualness."));
            WordKnowledgeCards.Add(new Flashcard("Menagerie", "A place where animals are kept and trained especially for exhibition."));
            WordKnowledgeCards.Add(new Flashcard("Charlatan", "A person who makes elaborate, fraudulent, and often voluble claims to skill or knowledge."));
            WordKnowledgeCards.Add(new Flashcard("Malingere", "To pretend or exaggerate incapacity or illness (as to avoid duty or work)."));
            WordKnowledgeCards.Add(new Flashcard("Dilatory", "Tending to cause delay."));
            WordKnowledgeCards.Add(new Flashcard("Sequester", "To set apart; segregate."));
            WordKnowledgeCards.Add(new Flashcard("Upbraid", "To criticize severely; find fault with."));
            WordKnowledgeCards.Add(new Flashcard("Antediluvian", "Of or relating to the period before the flood described in the Bible."));
            WordKnowledgeCards.Add(new Flashcard("Fastidious", "Showing or demanding excessive delicacy or care."));
            WordKnowledgeCards.Add(new Flashcard("Indolence", "Averse to activity, effort, or movement."));
            WordKnowledgeCards.Add(new Flashcard("Serendipity", "The faculty or phenomenon of finding valuable or agreeable things not sought for."));
            WordKnowledgeCards.Add(new Flashcard("Dilettante", "A person having a superficial interest in an art or a branch of knowledge."));
            WordKnowledgeCards.Add(new Flashcard("Chimerical", "Existing only as the product of unchecked imagination."));
            WordKnowledgeCards.Add(new Flashcard("Marred", "Impaired by wear, use, or damage."));
            WordKnowledgeCards.Add(new Flashcard("Paramour", "A lover, especially an illicit or secret one."));
            WordKnowledgeCards.Add(new Flashcard("Celerity", "Rapidity of motion or action."));
            WordKnowledgeCards.Add(new Flashcard("Maladroit", "Lacking skill, cleverness, or resourcefulness in handling situations."));
            WordKnowledgeCards.Add(new Flashcard("Scrumpulous", "Having moral integrity; acting in strict regard for what is considered right or proper."));
            WordKnowledgeCards.Add(new Flashcard("Didatic", "Designed or intended to teach."));
            WordKnowledgeCards.Add(new Flashcard("Prevaricate", "To deviate from the truth."));
            WordKnowledgeCards.Add(new Flashcard("Diffident", "Hesitant in acting or speaking through lack of self-confidence."));
            decks.Add(new CardDeck("Word Knowledge", WordKnowledgeCards));
        }
        static void ArchivedCards()
        {
            void SeminolePA44180()
            {
                List<Flashcard> flashcardz2 = new List<Flashcard>();
                //Aerodynamics
                flashcardz2.Add(new Flashcard("What aerodynamically happens during engine out?", "1. Pitch Down Motion (Lateral Axis)\n -Accelerated slipstream lessens on horizontal stabilizer, causing nose to drop\n\n2. Roll To Dead Engine (Longitudinal Axis)\n-Lift on side of operating engine induces roll towards dead engine. Aileron deflection towards operating engine is required.\n3. Yaw towards dead engine. (Vertical Axis) \n -Asymetrical thrust forces plane to yaw towards dead engine. Stepping on the ALIVE engine is required."));
                flashcardz2.Add(new Flashcard("What is climb performance dependent on, and how much climb performance is lost in Single Engine Ops?", "Dependent on excess power needed to overcome drag. Loss of engine removes 80% of climb performance."));
                flashcardz2.Add(new Flashcard("After losing engine, how does one get best climb performance?", "1. Maximize Thrust (Full Power)\n2. Minimize Drag (Flaps/Gear up. Prop Feathered, etc)"));
                flashcardz2.Add(new Flashcard("Piper Seminole Drag Factors", "1.Flaps 25 (-240FPM)\n2. Flaps 40 (-275FPM)\n3.Windmilling Prop (-300FPM)\n4.Gear Extended (-250FPM)"));
                flashcardz2.Add(new Flashcard("(FAR Part 23) The FAA does not require Multi-engines under 6,000lbs or Vso speed < 61kts to...", "Meet any specified single engine performance criteria. This is instead determined by the manufacturer."));
                flashcardz2.Add(new Flashcard("Seminole Max TOW\nSeminole Vso", "3,800lbs, 55kias"));
                flashcardz2.Add(new Flashcard("V-Speeds for Maximum Single Engine Performance", "Vxse: Airspeed for steepest single engine climb on single engine\nVyse: Airspeed for best rate of climb on single engine. (Or slowest loss of altitude on drift down)\nBlueline indicates at max weight"));
                flashcardz2.Add(new Flashcard("Sideslip vs Zero Sideslip", "Sideslip: Step on alive engine to maintain heading, but sideslip in this direction causes unwanted drag from relative wind.\n\nZero Sideslip (Optimal): Fuselage is aligned with relative wind. Bank (2-5 deg) into alive engine to achieve this. Furthermore, light rudder is necessary to balance the inclinometer."));
                flashcardz2.Add(new Flashcard("Single Engine Service Ceiling vs Absolute Ceiling", "Service: Max density altitude where Vyse will produce 50fpm climb with critical engine inop.\n\nAbsolute: Max density altitude aircraft maintains with 1 engine inop. Plane drifts down to this if above. Vyse and Vxse are equal here."));
                flashcardz2.Add(new Flashcard("Climb performance determines on 4 factors:", "Airspeed: Too little or too much decreases performance\nDrag: Flight Control Direction, Prop, Sideslip, Gear, Flaps, Cowl Flaps, \nPower: Amount available in excess of that needed for level flight.\nWeight: Pax, Bags, Fuel all decrease performance."));
                flashcardz2.Add(new Flashcard("Critical Engine Definition and which on Seminole?", "Critical Engine is the engine which loss will most adversely affect flight characteristics. The Seminole's engines are equally critical, as they are counterrotating."));
                flashcardz2.Add(new Flashcard("What factors usually cause clockwise (Viewed from cockpit) rotating engines to have a left critical engine?", "(PAST) P-Factor (Yaw): Descending blade has higher angle of attack, thus higher thrust at low speeds and high AOA. The right engine has a longer arm than left, thus more torque of yaw.\nAccelerated Slipstream (Roll/Pitch): The thrust as discussed before due to P-Factor causes a further arm on the right side. Therefore, this slipstream effect is more pronounced on the right side and causes more roll and pitch.\nSpiraling Slipstream (Yaw): Left engine slipstream hits vertical stabilizer and would counteract yaw of a dead right engine. Yet, right engine doesnt hit stabilizer and falls away, leaving dead left engine's yaw unchecked.\nTorque (Roll): Newton 3rd law. Clockwise rotating props cause counterclockwise roll. Dead left engine makes right provide more torque."));
                flashcardz2.Add(new Flashcard("Vmc. Marked as... Manufacturers determine by...", "Minimum speed that directional control can be maintained with single engine inoperative.\n\nRed radial line.\n\n1. Most unfavorable weight and CG (>Weight = >Horizontal Lift and Rudder Force which decreases Vmc) (Lengthened CG creates more downforce and thus lower Vmc)\n2. Standard day conditions at sea level (Max Engine Power) (Vmc decreases with altitude due to decreased yaw).\n3.Max Power on operative engine (Max Yaw) (Decrease of power on operative engine lowers Vmc)\n4.Critical Engine Prop Windmilling (Max Drag) (Feathered props Decrease drag and lower Vmc)\n5.Flaps Takeoff, Gear up, trimmed for takeoff. (Least Stability) (Keel effect of gear and stabalizing effect of flaps lower Vmc).\nUp to 5 Deg of bank into operative engine. (Vmc increases with decrease of bank by 3kts/deg)"));
                //Systems
                flashcardz2.Add(new Flashcard("Engine of Piper Seminole", "Lycoming, 4-Cylinder, 0-360 (Opposed, 360cu in) 180hp at 2700rpm. (LHAND)\nLycoming\nHorizontally opposed\nAir Cooled\nNormally Aspirated (No turbo, supercharging)\nDirect Drive (Crankshaft runs direct to prop)\n\nNotes: Piston driven, carburated, independent engine driven magnetos"));
                flashcardz2.Add(new Flashcard("When can carb icing occur? Signs? What to do?", "(20-70F/-5-20C)\n\nEngine Roughness and drop of manifold pressure\n\nCarb heat on, adjust mixture for smoothness"));
                flashcardz2.Add(new Flashcard("Seminole Propeller", "Harzell 2-Blade, Variable Pitch, Constant Speed, Full feathering, Metal Prop."));
                flashcardz2.Add(new Flashcard("What is controllable pitch? How does the process work?", "Ability to control engine RPM by varying blade pitch.\n\nA prop governer controls hydraulic oil through piston controlled by blue handle. Moving forward moves oil to low-pitch, high RPM (unfeathered) position. Backwards allows nitrogen-charged cylinder, spring, and centrifugal counterweight to move prop to high-pitch, low RPM (feathered) position."));
                flashcardz2.Add(new Flashcard("What is constant speed?", "RPM is unaffected by altitude or manifold pressure, since the prop goveneor controls oil in the hub accordingly to keep RPM."));
                flashcardz2.Add(new Flashcard("What is feathering and how is it done in Seminole?", "When prop blades are in alignment with the relative wind and thus reduce drag caused by prop blade area in wind.\nSimply move manifold pressure lever down into Feathering and allow 6 seconds. (Cutoff mixture to stop power production)"));
                flashcardz2.Add(new Flashcard("Explain the Seminole's centrifugal stop pin. ", "Prevents feathering below 950rpm to allow blades to remain in low pitch upon shutdown (Prevents excessive loads on engine starter next startup)\n\nRegardless of prop position, the prop will feather above 950RPM if oil pressure is lost."));
                flashcardz2.Add(new Flashcard("Explain Prop Overspeed. Explain the process upon seeing an overspeed.", "Caused by prop goveneor failure which allows blades to rotate to full low pitch.\nPull throttle to idle and prop control to full 'Decrease RPM'. Airspeed/throttle should be reduced to maintain 2700RPM."));
                flashcardz2.Add(new Flashcard("Seminole Landing Gear", "Hydraullically actuated, fully retractable, tricycle-type landing gear. Electrically powered, fully reverisble hydraulic pump provides hydraulic power. Gear is held up soley by this pressure. Springs help to lower and keep gear locked after downlock hooks engage, released only by hydraulic pressure from gear selector. "));
                flashcardz2.Add(new Flashcard("Explain the gear warning system", "Activated under ANY of the following:\n\n1. Level positioned below 15'' manifold pressure with gear not locked down.\n2.Gear not locked down with flaps at 25 or 40 deg.\n3. Gear handle up on ground."));
                flashcardz2.Add(new Flashcard("What happens when gear is retracted on ground?", "A squat switch on the left gear keeps a circuit open while extended, so this wouldnt occur."));
                flashcardz2.Add(new Flashcard("How to extend gear during hydraulic failure?", "Put gear knob down, then pull red emergency gear extension knob. This allows the gear to free-fall due to gravity. (Limited to 100kts)"));
                flashcardz2.Add(new Flashcard("How is nose gear controlled? Limits?", "Rudder pedals. 30 deg either side."));
                flashcardz2.Add(new Flashcard("Describe the Seminole's brakes?", "Hydraullically actuated disk brakes on the MAINS. Hydraulic fluid is independent of landing gear. Nose cone contains brake fluid reservoir."));
                flashcardz2.Add(new Flashcard("Describe the Seminole's flaps?", "Manual flaps controlled by lever. 0 - 10 - 25 - 40 degrees. Lever is spring loaded to return to 0. "));
                flashcardz2.Add(new Flashcard("Describe the Seminole's vacumn pumps?", "2 engine driven vacumn pumps. Control attitude gyro and HSI (On non-slaving mechanism aircraft) Limits: 4.8 - 5.2in Mercury at 2000RPM.\nFailure is noted by annunciator light and red pump inop indicator on vacumn gauge. Usually, the other pump should make up for the dead pump."));
                flashcardz2.Add(new Flashcard("Describe the Seminole's Pitot/Static system?", "A heated pitot mast (Combination of pitot tube and static port) is under the left wing. Alternate static is under left side instrument panel (Storm window/Cabin vents must be closed, heater/defroster on (Reduces differential and thus error))."));
                flashcardz2.Add(new Flashcard("Describe the Seminole's Fuel System?", "100LowLead Avgas (Blue), 2 55gal Bladder Nacelle tanks (1 gal unusable each), 2 engine driven and 2 electric driven pumps (Electric used for start, takeoff, landing, and fuel selector changes)"));
                flashcardz2.Add(new Flashcard("Describe the Seminole's Fuel Selector System?", "On (Normal Ops), Off, X-Feed (Fuel driven from opposite tank for engine) (Straight/Level flight only). Transferring fuel in flight is impossible."));
                flashcardz2.Add(new Flashcard("Explain how to Crossfeed the left engine to take fuel from right?", "1. Left tank boost pump on, X-Feed selected, Monitor Pressure, Pump off, Monitor Pressure"));
                flashcardz2.Add(new Flashcard("Describe the Seminole's Electrical System?", "14V system (12V, Lead-acid battery (Used for emergency and engine starts)), Push/Pull type breakers, 2 70-amp engine driven alternators (Voltage regulators manage 14V output at varying engine RPMs, effectively sharing electrical load.)"));
                flashcardz2.Add(new Flashcard("Describe alternator failure?", "Annunciator light and no load on loadmeter. Remaining alternator can provide adequate power."));
                flashcardz2.Add(new Flashcard("Describe the over-voltage relay?", "Protection system in each alternator. Will take the alternator offline if it exceeds 17V. "));
                flashcardz2.Add(new Flashcard("Describe the heater system", "Jantrol gas combustion heater in nose. Distrubuted by manifold to ducts to cabin floor and defroster. Controlled by 3 switches: 'Cabin Heat', 'Off', and 'Fan'. Airflow/Temp regulated by 3 levers: 'Air Intake', 'Temp', and 'Def'. 2 safety switches prevent operation with air intake in off position. Blower activates on ground. Fuel supplied at 1/2 gal/hr from left tank."));
                flashcardz2.Add(new Flashcard("Describe turning on cabin heat", "Air Intake lever/Cabin Heat switch must be on. Fuel flow starts and ignites heater. At selected 'Temp', the heater will cycle to maintain."));
                flashcardz2.Add(new Flashcard("Describe overheating sensor and resetting.", "There is an overheat switch aft inboard end of heater vent jacket that needs to be reset in nose by red button. 'Heater over temp' Annunciator will come on. To prevent overheat on ground, turn on fans and leave vents open."));
                flashcardz2.Add(new Flashcard("Describe the stall warning horns.", "2 electric horns on left wing. One inboard (For 25-40deg flaps) and one outboard (0-10deg) to provide adequate stall detection at different AOAs. These will deactivate on ground through squat switch."));
                flashcardz2.Add(new Flashcard("Describe the Seminole's emergency exit.", "Located on left door. Pull thermocover off, pull handle up, and push window out."));


                List<Flashcard> vSpeedCards = new List<Flashcard>();
                //Performance
                vSpeedCards.Add(new Flashcard("Vso", "55 (stall speed in landing) (bottom of white arc)"));
                vSpeedCards.Add(new Flashcard("Vmc", "56 (min controllable airspeed) (red radial line)"));
                vSpeedCards.Add(new Flashcard("Vs", "57 (stall speed zero flaps) (bottom green arc)"));
                vSpeedCards.Add(new Flashcard("Vr", "75 (rotation speed)"));
                vSpeedCards.Add(new Flashcard("Vx", "82 (best angle of climb)"));
                vSpeedCards.Add(new Flashcard("Vxse", "82 (best angle of climb single engine)"));
                vSpeedCards.Add(new Flashcard("Vyse", "82 (best rate of climb single engine)"));
                vSpeedCards.Add(new Flashcard("Vsse", "82 (safe speed for intentional engine failure)"));
                vSpeedCards.Add(new Flashcard("Vy", "88 (best rate of climb)"));
                vSpeedCards.Add(new Flashcard("Vyse", "88 (best rate of climb single engine) (blue line)"));
                vSpeedCards.Add(new Flashcard("Vfe", "111 (max flaps extension) (top of white arc)"));
                vSpeedCards.Add(new Flashcard("Vlo (up)", "109 (max gear retract speed)"));
                vSpeedCards.Add(new Flashcard("Vle", "140 (max gear speed)"));
                vSpeedCards.Add(new Flashcard("Vno", "169 (max structural cruise speed) (top of green arc)"));
                vSpeedCards.Add(new Flashcard("Vne", "202 (never exceed speed) (red line)"));
                vSpeedCards.Add(new Flashcard("Va", "135 (maneuvering speed (3800lbs))"));
                vSpeedCards.Add(new Flashcard("Va", "112 (maneuvering speed (2700lbs))"));
                vSpeedCards.Add(new Flashcard("Maximum demonstrated crosswind?", "17"));
                vSpeedCards.Add(new Flashcard("Cruise Climb", "105kts, 25in, 2,500rpm, 1000agl."));
                vSpeedCards.Add(new Flashcard("Maneuvers", "17in, 2,400rpm, 110kias."));
            }


            void Aerodynamics()
            {

            }

            void Aviation()
            {
                List<Flashcard> AviationInformationCards = new List<Flashcard>();
                // Aerodynamics
                AviationInformationCards.Add(new Flashcard("Adverse Yaw", "When plane yaws to outside of a turn. Caused by outside wing having lots of lift due to higher induced drag"));
                AviationInformationCards.Add(new Flashcard("Aerodynamics", "The science of action of air on an object, and with the motion of air on other gases. Aerodynamics deals with production of lift by the aircraft, the relative wind, and the atmosphere."));
                AviationInformationCards.Add(new Flashcard("Angle of Attack", "Angle between the chord line of the wing of an aircraft and the relative wind."));
                AviationInformationCards.Add(new Flashcard("Angle of Incidence", "The acute angle formed between the chord line of the wing and the longitudinal axis of the airplane."));
                AviationInformationCards.Add(new Flashcard("Asymmetric Thrust", "Thrust that is not equal on both sides of the aircraft."));
                AviationInformationCards.Add(new Flashcard("P-Factor (Yaw)", "Descend blade has higher angle of attack, thus higher thrust at low speeds and high AOA. The right engine has a longer arm than left, thus more torque of yaw."));
                AviationInformationCards.Add(new Flashcard("Axes of Aircraft", "Longitudinal (Roll), Lateral (Pitch), Vertical (Yaw)"));
                AviationInformationCards.Add(new Flashcard("Bernoulli's Principle", "The principle that states that as the velocity of a fluid increases, the pressure exerted by the fluid decreases."));
                AviationInformationCards.Add(new Flashcard("Calibrated Airspeed (CAS)", "The indicated airspeed of an aircraft corrected for position and instrument error."));
                AviationInformationCards.Add(new Flashcard("Camber", "The curvature of an airfoil from the leading edge to the trailing edge."));
                AviationInformationCards.Add(new Flashcard("Centrifugal Force", "The force that tends to move a rotating body away from the center of rotation."));
                AviationInformationCards.Add(new Flashcard("Centripedal Force", "The force that tends to move a rotating body toward the center of rotation. Horizontal component of lift"));
                AviationInformationCards.Add(new Flashcard("Coefficient of Lift", "The ratio of the lift produced by a wing to the product of the air density, the wing area, and the square of the velocity.\n\nAlso the ratio between lift pressure and dynamic pressure"));
                AviationInformationCards.Add(new Flashcard("Center of Pressure", "The point along the chord line of an airfoil where lift is considered to be concentrated. (Center of lift)"));
                AviationInformationCards.Add(new Flashcard("Critical Angle of Attack", "The angle of attack at which a wing stalls."));
                AviationInformationCards.Add(new Flashcard("Directional Stability", "The stability of an aircraft about its vertical axis, whereby aircraft tends to return to flight aligned with relative wind"));
                AviationInformationCards.Add(new Flashcard("Dutch Roll", "A combination of rolling and yawing oscillations that normally occurs when the dihedral effect is greater than the directional stability of an aircraft."));
                AviationInformationCards.Add(new Flashcard("Dynamic Stability", "The property of an aircraft that causes it to return to equilibrium after it has been disturbed."));
                AviationInformationCards.Add(new Flashcard("Ground Effect", "The result of the interference of the surface of the Earth with the airflow patterns about an airplane. One wing span away that increases pressure and therefore lift."));
                AviationInformationCards.Add(new Flashcard("Limit Load Factor", "The maximum load factor that an aircraft can sustain without any structural damage."));
                AviationInformationCards.Add(new Flashcard("Load Factor", "The ratio of the load supported by the airplane's wings to the actual weight of the aircraft and its contents."));
                AviationInformationCards.Add(new Flashcard("Magnus Effect", "The phenomenon that causes a spinning object to curve away from its principal flight path."));
                AviationInformationCards.Add(new Flashcard("Minimum Drag", "The point on the power curve where the aircraft has the least amount of drag."));
                AviationInformationCards.Add(new Flashcard("Negative Static Stability", "The condition of an aircraft in which it is directionally unstable and tends to continue away from the original flight path."));
                AviationInformationCards.Add(new Flashcard("Phugoid Oscillation", "A long-period longitudinal oscillation of an aircraft in which the aircraft alternately climbs and descends."));
                AviationInformationCards.Add(new Flashcard("Skidding Turn", "A turn in which the rate of turn is too great for the angle of bank, resulting in the aircraft slipping toward the outside of the turn."));

                // Navigation & Planning
                AviationInformationCards.Add(new Flashcard("Absolute Accuracy", "Ability to determine present position in space independently, and is used most often by pilots"));
                AviationInformationCards.Add(new Flashcard("Absolute Pressure", "Pressure measured from the reference of zero pressure, or a vacumn"));
                AviationInformationCards.Add(new Flashcard("Accelerate Go Distance", "The distance to accelerate to V1, experience engine failure, and climb 35 ft to V2"));
                AviationInformationCards.Add(new Flashcard("Accelerate Stop Distance", "Distance to V1, and complete stop"));
                AviationInformationCards.Add(new Flashcard("Agonic Line", "An irregular imaginary line across the surface of Earth along which the magnetic and geographic poles are in alignment, and along which there is no magnetic variation."));
                AviationInformationCards.Add(new Flashcard("Basic Empty Weight (BEW)", "The weight of the standard aircraft, including unusable fuel, full operating fluids, and full oil."));
                AviationInformationCards.Add(new Flashcard("Center of Gravity (CG)", "The point at which an aircraft would balance if it were possible to suspend it at that point. It is the mass center of the aircraft, or the theoretical point at which the entire weight of the aircraft is assumed to be concentrated."));
                AviationInformationCards.Add(new Flashcard("Compass Course", "True course corrected for variation and deviation."));
                AviationInformationCards.Add(new Flashcard("Critical Altitude", "The maximum altitude at which an aircraft can maintain a specified power setting."));
                AviationInformationCards.Add(new Flashcard("Dead Reckoning", "The process of determining one's present position by projecting course, speed, and elapsed time from a known past position."));
                AviationInformationCards.Add(new Flashcard("Density Altitude (DA)", "Pressure altitude corrected for nonstandard temperature."));
                AviationInformationCards.Add(new Flashcard("Deviation (Magnetic Compass)", "The error in a magnetic compass reading caused by local magnetic fields."));
                AviationInformationCards.Add(new Flashcard("Differential Global Positioning System (DGPS)", "A system that uses a network of ground-based reference stations to broadcast the difference between the positions indicated by the satellite systems and the known fixed positions. Improve GNSS"));
                AviationInformationCards.Add(new Flashcard("Equivalent Airpseed (EAS)", "The airspeed read directly from the airspeed indicator corrected Calibration and Sea Level."));
                AviationInformationCards.Add(new Flashcard("Floor-load Limit", "The maximum weight that can be carried in the cargo compartment."));
                AviationInformationCards.Add(new Flashcard("Flight Level", "A level of constant atmospheric pressure related to a reference datum of 29.92 inches of mercury, above 18,000MSL."));
                AviationInformationCards.Add(new Flashcard("Gyroscopic Precision", "Applied force responds to 90 deg deflection"));
                AviationInformationCards.Add(new Flashcard("Great Circle Route", "The shortest distance between two points on the surface of a sphere."));
                AviationInformationCards.Add(new Flashcard("Height above airport (HAA)", "The height of the MDA above the published airport elevation."));
                AviationInformationCards.Add(new Flashcard("Height above touchdown (HAT)", "The height of the MDA above the highest runway elevation in the touchdown zone."));
                AviationInformationCards.Add(new Flashcard("Horsepower", "The rate of doing work. 1 HP = 550 ft-lb/sec"));
                AviationInformationCards.Add(new Flashcard("Magnetic Dip", "The tendency for the compass needle to dip towards poles"));
                AviationInformationCards.Add(new Flashcard("Maximum Authorized Altitude (MAA)", "The highest altitude at which an aircraft may fly."));
                AviationInformationCards.Add(new Flashcard("Minimum Crossing Altitude (MCA)", "The lowest altitude at certain fixes at which an aircraft must cross when proceeding in the direction of a higher minimum enroute IFR altitude."));
                AviationInformationCards.Add(new Flashcard("Minimum Descent Altitude (MDA)", "The lowest altitude, expressed in feet above MSL, to which descent is authorized on final approach or during circle-to-land maneuvering."));
                AviationInformationCards.Add(new Flashcard("Minimum Enroute Altitude (MEA)", "The lowest published altitude between radio fixes that ensures acceptable navigational signal coverage and meets obstacle clearance requirements between those fixes."));
                AviationInformationCards.Add(new Flashcard("Minimum Obstruction Clearance Altitude (MOCA)", "The lowest published altitude in effect between radio fixes on VOR airways, off-airway routes, or route segments which meets obstacle clearance requirements for the entire route segment."));
                AviationInformationCards.Add(new Flashcard("Minimum Reception Altitude (MRA)", "The lowest altitude at which an intersection can be determined."));
                AviationInformationCards.Add(new Flashcard("Minimum Safe Altitude (MSA)", "The lowest altitude that provides a minimum clearance of 1,000 feet above all obstacles, and 2,000 feet in designated mountainous terrain."));
                AviationInformationCards.Add(new Flashcard("Minimum Vectoring Altitude (MVA)", "The lowest MSL altitude at which an IFR aircraft will be vectored by a radar controller, except as otherwise authorized for radar approaches, departures, and missed approaches."));
                AviationInformationCards.Add(new Flashcard("Pilotage", "Navigation by visual reference to landmarks."));
                AviationInformationCards.Add(new Flashcard("Variation", "The angular difference between true north and magnetic north."));

                // Charts
                AviationInformationCards.Add(new Flashcard("Isobars", "Lines on a weather map that connect points of equal pressure."));
                AviationInformationCards.Add(new Flashcard("Isogonal Lines", "Lines on a chart that connect points of equal magnetic variation."));
                AviationInformationCards.Add(new Flashcard("Jet Route", "A route designed to serve aircraft operations from 18,000 feet MSL to FL 450."));
                AviationInformationCards.Add(new Flashcard("Lines of Flux", "Imaginary lines used to describe the magnetic field around a magnet."));

                // Systems
                AviationInformationCards.Add(new Flashcard("Acceleration Errors", "Magnetic Compass error apparent when aircraft accelerates on east or west, causing compass to rotate north.\n\n- ANDS (Accelerate North Deaccelerate South)\n-NOSE (North Opposite, South Exaggerated) [When turning FROM north or south)"));
                AviationInformationCards.Add(new Flashcard("Accelerometer", "Part of INS (Inertial Navigation System) that accurately measures force of acceleration in 1 direction."));
                AviationInformationCards.Add(new Flashcard("ADF, ADI", "Automatic Direction Finder, Attitutude Direction Indicator"));
                AviationInformationCards.Add(new Flashcard("ADC", "Air Data Computer: An aicraft computer that recieves and processes pitot pressure, static pressure, and temperature to calculate very precise altitude, indicated airspeed, true airspeed, and temp."));
                AviationInformationCards.Add(new Flashcard("Adjustable Pitch Propeller", "Propeller with 2 blades whose pitch can be adjusted on the ground with engine not running, but cannot be adjusted in flight. Sometimes called Ground Adjustable Propeller. Constant Speed Props can be changed in flight."));
                AviationInformationCards.Add(new Flashcard("Adjustable Stabilizer", "Stabalizer which can be adjusted in flight to trim the aircraft, allowing aircraft to fly hands off at any given airspeed"));
                AviationInformationCards.Add(new Flashcard("ADS-B", "Automatic Dependent Surveillance-Broadcast: Sends WAAS-Enhanced GPS signal to ground controllers via towers"));
                AviationInformationCards.Add(new Flashcard("Ailerons", "Primary flight control surfaces mounted on trailing edge of an airplane wing, near the lip. Control roll about Longitudinal Axis."));
                AviationInformationCards.Add(new Flashcard("Airfoil", "Any surface, such as wing, prop, rudder, trim tab, that provides aerodynamic force when it interacts with a moving stream of air."));
                AviationInformationCards.Add(new Flashcard("Anti-servo Tab", "A tab connected to the trailing edge of a control surface to aid in the movement of the control surface. Moves in SAME direction of primary control."));
                AviationInformationCards.Add(new Flashcard("Axial Flow Compressor", "A type of compressor that uses a series of rotating and stationary airfoils to compress the air."));
                AviationInformationCards.Add(new Flashcard("Balance Tab", "A tab connected to the trailing edge of a control surface to aid in the movement of the control surface. Moves in OPPOSITE direction of primary control."));
                AviationInformationCards.Add(new Flashcard("Calibrated Orifice", "A small hole in the wall of a pitot tube that is used to measure the pressure of the air."));
                AviationInformationCards.Add(new Flashcard("Canard\n\nCanard Configuration", "A configuration in which the horizontal stabilizer is mounted ahead of the main wing.\n\n- When the span of the forward wings is considerably less than that of the main wings"));
                AviationInformationCards.Add(new Flashcard("Cantilever Wing", "A wing that is attached to the fuselage without external bracing."));
                AviationInformationCards.Add(new Flashcard("Centrifugal Flow Compressor", "A type of compressor that uses a series of rotating and stationary airfoils to compress the air."));
                AviationInformationCards.Add(new Flashcard("Compass Locator", "A low-power, low or medium frequency radio beacon installed at the outer/middle marker of an ILS."));
                AviationInformationCards.Add(new Flashcard("Complex Airplane", "An airplane that has retractable landing gear, flaps, and a controllable pitch propeller."));
                AviationInformationCards.Add(new Flashcard("Compressor Stall", "A condition in a gas turbine engine in which the airflow through the compressor is disrupted, causing a loss of power or damage."));
                AviationInformationCards.Add(new Flashcard("Controllability", "The capability of an aircraft to respond to the pilots control inputs, especially with regard to flight path and altitude."));
                AviationInformationCards.Add(new Flashcard("Control Pressures", "The forces exerted by the pilot on the controls to achieve desired altitude"));
                AviationInformationCards.Add(new Flashcard("Conventional Gear", "An airplane that has a tailwheel."));
                AviationInformationCards.Add(new Flashcard("Coupled Ailerons and Rudder", "A system that automatically coordinates the movement of the ailerons and rudder to provide coordinated turns. (Counteract Adverse Yaw)"));
                AviationInformationCards.Add(new Flashcard("Cowl Flaps", "Adjustable air intakes in the engine cowling that are used to control engine temperature."));
                AviationInformationCards.Add(new Flashcard("Current Induction", "The flow of electrical current in a conductor caused by a moving magnetic field."));
                AviationInformationCards.Add(new Flashcard("Differential Ailerons", "Ailerons that are rigged so that the aileron moving upward travels a greater distance than the aileron moving downward. (Minimize Adverse Yaw)"));
                AviationInformationCards.Add(new Flashcard("Dihedral Wings", "Acute angle between lateral axis and wings. Increases lateral stability."));
                AviationInformationCards.Add(new Flashcard("Dynamic Hydroplaning", "A condition that exists when the standing water goes deeper than tread of tires. Use anti-skid"));
                AviationInformationCards.Add(new Flashcard("Eddy Currents", "Currents induced in a conductor by a moving magnetic field."));
                AviationInformationCards.Add(new Flashcard("Eddy Current Dampening", "A method of reducing the oscillations of a gyroscope by passing an alternating current through coils in the gyro."));
                AviationInformationCards.Add(new Flashcard("Electronic Flight Display (EFD)", "A display system that presents flight data on a video screen."));
                AviationInformationCards.Add(new Flashcard("Empennage", "The entire tail group of an aircraft, including the horizontal and vertical stabilizers, the elevators, and the rudder."));
                AviationInformationCards.Add(new Flashcard("Encoding Altimeter", "An altimeter that transmits a signal to the transponder to provide air traffic controllers with the aircraft's altitude."));
                AviationInformationCards.Add(new Flashcard("Engine Pressure Ratio (EPR)", "The ratio of the turbine discharge pressure to the compressor inlet pressure."));
                AviationInformationCards.Add(new Flashcard("Fixed-Pitch Propeller", "A propeller that has a fixed blade angle and is not adjustable."));
                AviationInformationCards.Add(new Flashcard("Fixed Slot", "A fixed aerodynamic device that is used to improve the low-speed characteristics of an airfoil."));
                AviationInformationCards.Add(new Flashcard("Flameout", "The extinction of the flame in the combustion chamber of a turbine engine."));
                AviationInformationCards.Add(new Flashcard("Flaps", "A high-lift device that increases the camber of the wing, thus increasing lift."));
                AviationInformationCards.Add(new Flashcard("Flight Director Indicator (FDI)", "A flight instrument that provides the pilot with guidance for flying a specific flight path."));
                AviationInformationCards.Add(new Flashcard("Flight Management System (FMS)", "A computer system that uses a large database to allow pilots to enter a flight plan and then automatically fly the aircraft along that route."));
                AviationInformationCards.Add(new Flashcard("Frise-Type Aileron", "An aileron that is designed to create drag when it is deflected upward. The nose creates parasitic drag and decreases adverse yaw"));
                AviationInformationCards.Add(new Flashcard("Ground-Adjustable Trim Tab", "A trim tab that can be adjusted on the ground, but not in flight."));
                AviationInformationCards.Add(new Flashcard("Ground Proximity Warning System (GPWS)", "A system that uses radar to determine the aircraft's altitude and warn the pilot if the aircraft is in danger of flying into the ground."));
                AviationInformationCards.Add(new Flashcard("High performance Airplane", "An airplane with an engine of more than 200 horsepower."));
                AviationInformationCards.Add(new Flashcard("Inertial Navigation System (INS)", "A navigation system that uses a computer, accelerometers, and gyroscopes to continuously calculate the position, orientation, and velocity of an aircraft without the need for external references."));
                AviationInformationCards.Add(new Flashcard("Instantaneous Vertical Speed Indicator (IVSI)", "A vertical speed indicator that responds to changes in altitude almost immediately."));
                AviationInformationCards.Add(new Flashcard("Lead Radial", "The radial on which the aircraft intercepts the final approach course."));
                AviationInformationCards.Add(new Flashcard("Manifold Absolute Pressure (MAP)", "The absolute pressure of the fuel/air mixture in the intake manifold."));
                AviationInformationCards.Add(new Flashcard("Slat", "A high-lift device that is mounted on the leading edge of the wing and is used to increase lift at low speeds."));
                AviationInformationCards.Add(new Flashcard("Overboost", "A condition in which the pressure in the intake manifold of a reciprocating engine exceeds the manufacturer's specifications."));
                AviationInformationCards.Add(new Flashcard("Planform", "The shape and layout of an airplane's wing as viewed from above."));

                // ADM
                AviationInformationCards.Add(new Flashcard("ADM", "Aeronautical Decision Making: Systematic approach to mental process used by pilots to consistently determine the best course of action in response to a given set of circumstances. Hazardous Attitudes: \nMacho: I can do it\nImpulsivity: Do it quickly\nAntiauthority: Dont tell me what to do\nInvulnerability: It wont happen to me\nResignation: Whats the use?"));
                AviationInformationCards.Add(new Flashcard("International Civil Aviation Organization (ICAO)", "A specialized agency of the United Nations that codifies the principles and techniques of international air navigation and fosters the planning and development of international air transport to ensure safe and orderly growth."));

                // Weather
                AviationInformationCards.Add(new Flashcard("Advection Fog", "Warm movement over cold surface"));
                AviationInformationCards.Add(new Flashcard("AIRMET", "In flight weather advisory issued in amendment to area forecast, concerning weather phenomena of operational interest to all aircraft and potentially hazardous to aircraft with limited capability."));
                AviationInformationCards.Add(new Flashcard("Area Forecast (FA)", "A forecast of general weather conditions over a large area."));
                AviationInformationCards.Add(new Flashcard("Automated Surface Observing System (ASOS)", "An automated weather observing system that provides continuous minute-by-minute observations of the weather."));
                AviationInformationCards.Add(new Flashcard("Automatic Terminal Information Service (ATIS)", "The continuous broadcast of recorded noncontrol information in selected high-activity terminal areas."));
                AviationInformationCards.Add(new Flashcard("Aviation Routine Weather Report (METAR)", "An aviation weather report issued at hourly or half-hourly intervals."));
                AviationInformationCards.Add(new Flashcard("Convective Weather", "Unstable, rising air found in cumuliform clouds, thunderstorms, and associated weather phenomena."));

                // Instrument
                AviationInformationCards.Add(new Flashcard("Approach Category", "Based on 1.3x Stall Speed in landing configuration at maximum gross landing weight."));
                AviationInformationCards.Add(new Flashcard("Airport Markings", ""));
                AviationInformationCards.Add(new Flashcard("Airport Signs", ""));
                AviationInformationCards.Add(new Flashcard("Airport Surface Detection Equipment (ASDE)", "Radar equipment specifically designed to detect all principal features and traffic on the surface of the airport, presenting the entire image on the control tower console, used to augment visual observation by tower personnel of aircraft and or vehicular movements on runways or taxiways"));
                AviationInformationCards.Add(new Flashcard("Airport Surveillance Radar (ASR)", "Approach control radar to detect and display an aircrafts position in the terminal area."));
                AviationInformationCards.Add(new Flashcard("Air Route Surveillance Radar (ARSR)", "Long range radar used to detect and display an aircrafts position in the enroute phase of flight."));
                AviationInformationCards.Add(new Flashcard("Air Traffic Control (ATC)", "A service operated by appropriate authority to promote the safe, orderly, and expeditious flow of air traffic."));
                AviationInformationCards.Add(new Flashcard("Air Route Traffic Control Center (ARTCC)", "Facility established to provide air traffic control service to aircraft operating on IFR flight plans within controlled airspace and principally during the enroute phase of flight between terminal areas."));
                AviationInformationCards.Add(new Flashcard("Air Traffic Control Radar Beacon System (ATCRBS)", "Secondary Surveillance Radar (SSR) system that uses transponder transmitters aboard aircraft and ground radar to detect and display aircraft position."));
                AviationInformationCards.Add(new Flashcard("Airway", "A control area or route in the form of a corridor established for the flight of aircraft."));
                AviationInformationCards.Add(new Flashcard("Alert Area", "Area of high volume of pilot training or an unusual type of aerial activity."));
                AviationInformationCards.Add(new Flashcard("Alternate Airport", "An airport at which an aircraft may land if a landing at the intended airport becomes inadvisable."));
                AviationInformationCards.Add(new Flashcard("Alternate Static Source Valve", "Valve that allows the pilot to select an alternate static source in the event the primary source becomes clogged."));
                AviationInformationCards.Add(new Flashcard("Altimeter Setting", "Value to which the barometric pressure scale of the altimeter is set so that it will indicate true altitude at field elevation."));
                AviationInformationCards.Add(new Flashcard("Aneroid", "A small, sealed, flexible-walled, evacuated capsule used to measure pressure or pressure changes within the capsule."));
                AviationInformationCards.Add(new Flashcard("Aneroid Barometer", "An instrument for measuring atmospheric pressure."));
                AviationInformationCards.Add(new Flashcard("CDI", "Course Deviation Indicator: A cockpit instrument that displays the aircraft's lateral position in relation to a selected course."));
                AviationInformationCards.Add(new Flashcard("Changeover Point", "The point at which the aircraft changes from the outbound course to the inbound course on an omnirange approach."));
                AviationInformationCards.Add(new Flashcard("Clearance on Request (COR)", "An ATC clearance issued to a pilot after filing flight plan"));
                AviationInformationCards.Add(new Flashcard("Cocentric Rings", "A series of circles around a VOR that are used to depict distance from the VOR."));
                AviationInformationCards.Add(new Flashcard("Control Display Unit (CDU)", "Display interfaced with master computer, providing pilot with single control point"));
                AviationInformationCards.Add(new Flashcard("Cruise Clearance", "ATC clearance to cruise at a specified altitude"));
                AviationInformationCards.Add(new Flashcard("Decision Altitude (DA)", "The altitude at which a missed approach must be initiated if the required visual reference to continue the approach has not been established. (MSL)"));
                AviationInformationCards.Add(new Flashcard("Decision Height (DH)", "The height at which a decision must be made during an ILS or PAR approach. (Height above threshold elevation)"));
                AviationInformationCards.Add(new Flashcard("Departure Procedure (DP)", "A preplanned instrument flight rule (IFR) air traffic control departure procedure printed for pilot use."));
                AviationInformationCards.Add(new Flashcard("Doghouse", "A visual indicator on the attitude indicator that shows the aircraft's angle of bank."));
                AviationInformationCards.Add(new Flashcard("Duplex", "Sending on one frequency and receiving on another."));
                AviationInformationCards.Add(new Flashcard("Federal Airways", "Class E airspace extending upward from 1,200 feet above the surface of the earth (to but NOT including 18,000 MSL) and designated for enroute use of IFR traffic."));
                AviationInformationCards.Add(new Flashcard("Feeder Route", "A route designed to conduct aircraft from the enroute structure to the terminal area."));
                AviationInformationCards.Add(new Flashcard("ILS Categories", "Category I: 200 ft, 1/2 mile\nCategory II: 100 ft, 1/4 mile\nCategory III: 50 ft, 700 ft"));
                AviationInformationCards.Add(new Flashcard("Local Area Augmentation System (LAAS)", "A system that augments the Global Positioning System (GPS) to provide precision approach guidance to aircraft."));
                AviationInformationCards.Add(new Flashcard("Obstacle Departure Procedure (ODP)", "A preplanned instrument flight rule (IFR) air traffic control departure procedure printed for pilot use. May be flown without clearance"));
                AviationInformationCards.Add(new Flashcard("Prevailing Visibility", "The greatest horizontal visibility equaled or exceeded throughout at least half the horizon circle."));

                AviationInformationCards.Add(new Flashcard("Approach Lighting System (ALS)", "A lighting system installed on the approach end of an airport runway and consisting of a series of light bars, strobe lights, or a combination of the two."));
                AviationInformationCards.Add(new Flashcard("Area Charts", "Charts that provide a large-scale portrayal of navigational and topographic information."));
                AviationInformationCards.Add(new Flashcard("Area Navigation (RNAV)", "A method of navigation that permits aircraft operation on any desired flight path."));
                AviationInformationCards.Add(new Flashcard("Atittude and Heading Reference System (AHRS)", "A system that provides the aircrafts primary flight control system with attitude information."));
                AviationInformationCards.Add(new Flashcard("Attitude Direction Indicator (ADI)", "An instrument that displays the aircrafts attitude."));

                // Human Factors
                AviationInformationCards.Add(new Flashcard("Autokenesis", "The apparent movement of a stationary light when stared at for a period of time."));
                AviationInformationCards.Add(new Flashcard("Coriolis Illusion", "An illusion that occurs when a pilot has been in a turn long enough for the fluid in the inner ear to stop moving, and the pilot makes a head movement. This causes the pilot to believe the aircraft is rotating or accelerating in an entirely different axis."));
                AviationInformationCards.Add(new Flashcard("Elevator Illusion", "An illusion that occurs when the aircraft is subjected to an upward vertical acceleration. The pilot may falsely believe that the aircraft is in a nose-up attitude."));
                AviationInformationCards.Add(new Flashcard("Empty-Field Myopia", "An illusion that occurs when the pilot focuses on a featureless (dark) expanse of ground or water."));
                AviationInformationCards.Add(new Flashcard("Graveyard Spiral", "A spiral dive entered into and maintained by a pilot who becomes disoriented during a coordinated, constant-rate turn."));
                AviationInformationCards.Add(new Flashcard("Inversion Illusion", "Abrupt change from climb to straight and level makes pilot feel like tumbling backwards"));
                AviationInformationCards.Add(new Flashcard("Leans", "An illusion that occurs when a pilot has been in a turn long enough for the fluid in the inner ear to stop moving, and the pilot returns to level flight. The pilot may falsely believe that the aircraft is in a bank in the opposite direction of the original turn."));
                AviationInformationCards.Add(new Flashcard("Somatogravic Illusion", "An illusion that occurs when the aircraft accelerates on takeoff. The pilot may falsely believe that the aircraft is in a nose-up attitude."));

                AviationInformationCards.Add(new Flashcard("Diluter Demand Oxygen System", "A system that supplies oxygen to the user at cabin pressure altitude above 25,000 feet."));
                AviationInformationCards.Add(new Flashcard("Pressure Demand Oxygen System", "A system that supplies oxygen to the user at cabin pressure altitude above 34,000 feet."));
                AviationInformationCards.Add(new Flashcard("Hypoxic Hypoxia", "A condition resulting from a lack of oxygen in the body's tissues."));
                AviationInformationCards.Add(new Flashcard("Hypemic Hypoxia", "A condition resulting from the inability of the blood to carry oxygen to the cells. CO"));
                AviationInformationCards.Add(new Flashcard("Histotoxic Hypoxia", "A condition resulting from the inability of the cells to effectively use oxygen. Cigs"));
                AviationInformationCards.Add(new Flashcard("Stagnant Hypoxia", "A condition resulting from the inability of the blood to carry oxygen to the cells."));
            }

            void Arith()
            {
                List<Flashcard> ArithmeticReasoningCards = new List<Flashcard>();
                // Integers
                ArithmeticReasoningCards.Add(new Flashcard("Integer", "A whole number that is not a fraction or a decimal (-1,0,1)"));
                ArithmeticReasoningCards.Add(new Flashcard("Whole Number", "A number without fractions or decimals (0,1,2,3)"));
                ArithmeticReasoningCards.Add(new Flashcard("Natural Number", "A number that occurs naturally (1,2,3)"));
                ArithmeticReasoningCards.Add(new Flashcard("Prime Number", "A number that is only divisible by 1 and itself (2,3,5,7,11,13,17,19,23,29,31,37,41,43,47,53,59,61,67,71,73,79,83,89,97)"));
                ArithmeticReasoningCards.Add(new Flashcard("Composite Number", "A number that is divisible by more than just 1 and itself (4,6,8,9,10,12,14,15,16,18,20)"));
                ArithmeticReasoningCards.Add(new Flashcard("Even Number", "A number that is divisible by 2 (0,2,4,6,8,10,12,14,16,18,20)"));
                ArithmeticReasoningCards.Add(new Flashcard("Odd Number", "A number that is not divisible by 2 (1,3,5,7,9,11,13,15,17,19,21)"));
                ArithmeticReasoningCards.Add(new Flashcard("Rational Number", "A number that can be expressed as a fraction (1/2, 3/4, 5/6)"));
                ArithmeticReasoningCards.Add(new Flashcard("Irrational Number", "A number that cannot be expressed as a fraction (pi, e)"));
                ArithmeticReasoningCards.Add(new Flashcard("Real Number", "A number that can be found on the number line (0,1,2,3,4,5,6,7,8,9,10)"));
                ArithmeticReasoningCards.Add(new Flashcard("Imaginary Number", "A number that is not real (i)"));
                ArithmeticReasoningCards.Add(new Flashcard("Complex Number", "A number that is a combination of a real and imaginary number (a + bi)"));
                // Order of Operations
                ArithmeticReasoningCards.Add(new Flashcard("Order of Operations", "PEMDAS: Parentheses, Exponents, Multiplication, Division, Addition, Subtraction"));
                ArithmeticReasoningCards.Add(new Flashcard("What is 2 + (2 x 4)^2 x 55 - (3/5)?", "2 + 32 x 55 - 0.6 = 1759.4"));
                // Greatest Common Factor (GCF)
                ArithmeticReasoningCards.Add(new Flashcard("Greatest Common Factor", "The largest number that divides evenly into two or more numbers"));  /////////////////////
                ArithmeticReasoningCards.Add(new Flashcard("What is the GCF of 24 and 36?", "12"));
                // Least Common Multiple (LCM)
                ArithmeticReasoningCards.Add(new Flashcard("Least Common Multiple", "The smallest number that is a multiple of two or more numbers"));  /////////////////////
                ArithmeticReasoningCards.Add(new Flashcard("What is the LCM of 24 and 36?", "72"));
                // Exponents
                ArithmeticReasoningCards.Add(new Flashcard("Multiply Same Base Exponents", "Add the exponents"));
                ArithmeticReasoningCards.Add(new Flashcard("Divide Same Base Exponents", "Subtract the exponents"));
                ArithmeticReasoningCards.Add(new Flashcard("Raise Exponent to Exponent", "Multiply the exponents"));
                ArithmeticReasoningCards.Add(new Flashcard("Raise Exponent to Negative Exponent", "Reciprocal of the base raised to the positive exponent"));
                ArithmeticReasoningCards.Add(new Flashcard("Raise Exponent to 0", "1"));
                //Roots
                ArithmeticReasoningCards.Add(new Flashcard("Square Root", "A number that produces a specified quantity when multiplied by itself"));
                ArithmeticReasoningCards.Add(new Flashcard("Cube Root", "A number that produces a specified quantity when multiplied by itself twice"));
                ArithmeticReasoningCards.Add(new Flashcard("What is the square root of 144?", "12"));
                // Roots/Radicals
                ArithmeticReasoningCards.Add(new Flashcard("Number under the radical", "Radicand"));
                ArithmeticReasoningCards.Add(new Flashcard("How to simplify a radical", "Find the prime factors of the radicand, then pair them in groups of 2")); /////////////////////
                                                                                                                                                                   // Add/Subtract/Multiply/Divide Fractions
                ArithmeticReasoningCards.Add(new Flashcard("Add/Subtract Fractions", "Find a common denominator, then add/subtract the numerators"));
                ArithmeticReasoningCards.Add(new Flashcard("Multiply Fractions", "Multiply the numerators, then multiply the denominators"));
                ArithmeticReasoningCards.Add(new Flashcard("Divide Fractions", "Multiply the first fraction by the reciprocal of the second fraction"));
                // Add/Subtract/Multiply/Divide Decimals
                ArithmeticReasoningCards.Add(new Flashcard("Add/Subtract Decimals", "Line up the decimal points, then add/subtract as normal"));
                ArithmeticReasoningCards.Add(new Flashcard("Multiply Decimals", "Multiply as normal, then count the total number of decimal places in the factors and place that many decimal places in the product"));
                ArithmeticReasoningCards.Add(new Flashcard("Divide Decimals", "Move the decimal point in the divisor to the right until it is a whole number, then move the decimal point in the dividend the same number of places to the right"));
                // Decimal to Fraction
                ArithmeticReasoningCards.Add(new Flashcard("Decimal to Fraction", "Write the decimal as a fraction, then simplify"));
                ArithmeticReasoningCards.Add(new Flashcard("What is 0.245 as a fraction?", "49/200"));
                ArithmeticReasoningCards.Add(new Flashcard("What is 0.123 as a fraction?", "123/1000"));
                ArithmeticReasoningCards.Add(new Flashcard("What is 0.75 as a fraction?", "3/4"));
                // Percentages
                ArithmeticReasoningCards.Add(new Flashcard("Percentage Discount", "100% - (New Price/Old Price)"));
                // Mean, Median, Mode, Range
                ArithmeticReasoningCards.Add(new Flashcard("Mean", "Average of a set of numbers (Total Numbers Added / Total Count of Nums)"));
                ArithmeticReasoningCards.Add(new Flashcard("What is the Mean of 24, 23, 9, 129, 48?", "46.66"));
                ArithmeticReasoningCards.Add(new Flashcard("Median", "Middle number of a set of numbers ARRANGED IN ORDER OF NUMERIC VALUE (If even, average of 2 middle numbers)"));
                ArithmeticReasoningCards.Add(new Flashcard("What is the median of 24, 23, 9, 129, 48?", "24"));
                ArithmeticReasoningCards.Add(new Flashcard("What is the median of 24, 23, 9, 129, 48, 50?", "36"));
                ArithmeticReasoningCards.Add(new Flashcard("Mode", "Number that appears most frequently in a set of numbers (If multiple of same num occurances, set is bimodial)"));
                ArithmeticReasoningCards.Add(new Flashcard("What is the mode of 24, 23, 9, 129, 48?", "No Mode"));
                ArithmeticReasoningCards.Add(new Flashcard("What is the mode of 24, 24, 23, 9, 129, 48?", "24"));
                ArithmeticReasoningCards.Add(new Flashcard("What is the mode of 24, 24, 23, 23, 9, 129, 48?", "24, 23 - Bimodial"));
                ArithmeticReasoningCards.Add(new Flashcard("Range", "Difference between the highest and lowest numbers in a set of numbers"));
                ArithmeticReasoningCards.Add(new Flashcard("What is the range of 24, 23, 9, 129, 48?", "120"));
                // Scientific Notation
                ArithmeticReasoningCards.Add(new Flashcard("Scientific Notation", "A method of expressing a quantity as a number multiplied by 10 to the appropriate power."));
                ArithmeticReasoningCards.Add(new Flashcard("3.14 x 10^3", "3140"));
                ArithmeticReasoningCards.Add(new Flashcard("3.14 x 10^-3", "0.00314"));
            }


            void MathKnowledge()
            {

                List<Flashcard> MathematicsKnowledgeCards = new List<Flashcard>();
                // Algebra
                MathematicsKnowledgeCards.Add(new Flashcard("Algebra", "A branch of mathematics that substitutes letters for numbers."));
                MathematicsKnowledgeCards.Add(new Flashcard("Variable", "A symbol for a number we don't know yet. It is usually a letter like x or y."));
                MathematicsKnowledgeCards.Add(new Flashcard("Expression", "A mathematical phrase that can contain ordinary numbers, variables, and operators."));
                MathematicsKnowledgeCards.Add(new Flashcard("Equation", "A mathematical statement that two expressions are equal."));
                MathematicsKnowledgeCards.Add(new Flashcard("Inequality", "A mathematical statement that two expressions are not equal."));
                MathematicsKnowledgeCards.Add(new Flashcard("Function", "A relation between a set of inputs and a set of permissible outputs with the property that each input is related to exactly one output."));
                MathematicsKnowledgeCards.Add(new Flashcard("Quadratic Equation", "An equation of the second degree, meaning it contains at least one term that is squared."));
                MathematicsKnowledgeCards.Add(new Flashcard("Linear Equation", "An equation between two variables that gives a straight line when plotted on a graph."));
                MathematicsKnowledgeCards.Add(new Flashcard("Polynomial", "An expression consisting of variables and coefficients, that involves only the operations of addition, subtraction, multiplication, and non-negative integer exponents of variables."));
                MathematicsKnowledgeCards.Add(new Flashcard("Exponential Function", "A function of the form f(x) = a^x, where a is a positive real number not equal to 1."));
                MathematicsKnowledgeCards.Add(new Flashcard("Logarithm", "The power to which a base must be raised to produce a given number."));
                MathematicsKnowledgeCards.Add(new Flashcard("Slope", "The steepness of a line, defined as the ratio of the vertical change to the horizontal change."));
                MathematicsKnowledgeCards.Add(new Flashcard("Y-Intercept", "The point where the graph of a function or an equation crosses the y-axis."));
                MathematicsKnowledgeCards.Add(new Flashcard("X-Intercept", "The point where the graph of a function or an equation crosses the x-axis."));
                MathematicsKnowledgeCards.Add(new Flashcard("Slope-Intercept Form", "y = mx + b"));
                MathematicsKnowledgeCards.Add(new Flashcard("Point-Slope Form", "y - y1 = m(x - x1)"));
                MathematicsKnowledgeCards.Add(new Flashcard("Standard Form", "Ax + By = C"));
                MathematicsKnowledgeCards.Add(new Flashcard("Quadratic Formula", "x = (-b ± √(b^2 - 4ac)) / 2a"));
                MathematicsKnowledgeCards.Add(new Flashcard("Completing the Square", "A process used to solve a quadratic equation by rewriting the equation in the form (x - h)^2 = k."));
                MathematicsKnowledgeCards.Add(new Flashcard("Factoring", "The process of finding the factors of a number or expression."));
                MathematicsKnowledgeCards.Add(new Flashcard("Rational Function", "A function that can be written as the quotient of two polynomial functions."));
                MathematicsKnowledgeCards.Add(new Flashcard("Absolute Value", "The distance of a number from zero."));
                MathematicsKnowledgeCards.Add(new Flashcard("Absolute Value Function", "A function that contains an algebraic expression within absolute value symbols."));
                MathematicsKnowledgeCards.Add(new Flashcard("Complex Number", "A number that is a combination of a real and imaginary number."));
                MathematicsKnowledgeCards.Add(new Flashcard("Conjugate", "The complex number a - bi is the conjugate of a + bi."));
                MathematicsKnowledgeCards.Add(new Flashcard("Imaginary Unit", "The square root of -1."));
                MathematicsKnowledgeCards.Add(new Flashcard("Imaginary Number", "A number that is not real."));
                MathematicsKnowledgeCards.Add(new Flashcard("Real Number", "A number that can be found on the number line."));
                MathematicsKnowledgeCards.Add(new Flashcard("Complex Plane", "A plane with two perpendicular real axes and two perpendicular imaginary axes."));

                MathematicsKnowledgeCards.Add(new Flashcard("Complex Conjugate", "The complex number a - bi is the conjugate of a + bi."));
                MathematicsKnowledgeCards.Add(new Flashcard("Imaginary Unit", "The square root of -1."));
                MathematicsKnowledgeCards.Add(new Flashcard("Imaginary Number", "A number that is not real."));
                MathematicsKnowledgeCards.Add(new Flashcard("Real Number", "A number that can be found on the number line."));
                MathematicsKnowledgeCards.Add(new Flashcard("Complex Plane", "A plane with two perpendicular real axes and two perpendicular imaginary axes."));

                // Geometry
                MathematicsKnowledgeCards.Add(new Flashcard("Geometry", "A branch of mathematics that deals with points, lines, angles, surfaces, and solids."));
                MathematicsKnowledgeCards.Add(new Flashcard("Point", "A location in space."));
                MathematicsKnowledgeCards.Add(new Flashcard("Line", "A straight path that extends without end in opposite directions."));
                // Geometry Formulas
                MathematicsKnowledgeCards.Add(new Flashcard("Area of a Circle", "πr^2"));
                MathematicsKnowledgeCards.Add(new Flashcard("Circumference of a Circle", "2πr"));
                MathematicsKnowledgeCards.Add(new Flashcard("Area of a Triangle", "1/2bh"));
                MathematicsKnowledgeCards.Add(new Flashcard("Area of a Rectangle", "lw"));
                MathematicsKnowledgeCards.Add(new Flashcard("Area of a Square", "s^2"));
                MathematicsKnowledgeCards.Add(new Flashcard("Area of a Parallelogram", "bh"));
                MathematicsKnowledgeCards.Add(new Flashcard("Area of a Trapezoid", "1/2h(b1 + b2)"));
                MathematicsKnowledgeCards.Add(new Flashcard("Volume of a Cylinder", "πr^2h"));
                MathematicsKnowledgeCards.Add(new Flashcard("Volume of a Sphere", "4/3πr^3"));
                MathematicsKnowledgeCards.Add(new Flashcard("Volume of a Cone", "1/3πr^2h"));
                MathematicsKnowledgeCards.Add(new Flashcard("Volume of a Rectangular Solid", "lwh"));
                MathematicsKnowledgeCards.Add(new Flashcard("Volume of a Cube", "s^3"));
                MathematicsKnowledgeCards.Add(new Flashcard("Volume of a Prism", "Bh"));
            }
        }



        static void Save()
        {
            jsSave.Save(JSONFilePath, decks);
        }

        static void Load()
        {
            jsSave.Load(JSONFilePath, out decks);
        }
    }
}

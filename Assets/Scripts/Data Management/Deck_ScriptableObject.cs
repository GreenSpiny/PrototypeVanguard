using UnityEngine;

[CreateAssetMenu(fileName = "New Deck", menuName = "Custom/CreateNewDeck", order = 1)]
public class Deck_ScriptableObject : ScriptableObject
{
    // All values other than the deck name are IDs referencing the actual sleeves or card.

    public string deckName;         // Name of the deck.
    public string cardSleeves;      // ID linking to the card sleeves.

    public string[] mainDeck;   // MAIN DECK.                   50 cards max
    public string[] rideDeck;   // RIDE DECK.                   5  cards max
    public string[] strideDeck; // STRIDE DECK.                 16 cards max
    public string[] crests;     // CRESTS.                      4  cards max
    public string[] toolBox;    // TOKENS, TICKETS, MARKERS.    25 cards max
                                //                            = 100 total
}

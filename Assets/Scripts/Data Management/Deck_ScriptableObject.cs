using UnityEngine;

[CreateAssetMenu(fileName = "New Deck", menuName = "Custom/CreateNewDeck", order = 1)]
[System.Serializable]
public class Deck_ScriptableObject : ScriptableObject
{
    // All values other than the deck name are IDs referencing the actual sleeves or card.

    public string deckName;     // Name of the deck.
    public int cardSleeves;     // ID linking to the card sleeves.

    public int[] mainDeck;      // MAIN DECK.                   50 cards max
    public int[] rideDeck;      // RIDE DECK.                   5  cards max
    public int[] strideDeck;    // STRIDE DECK.                 16 cards max
    public int[] crests;        // CRESTS.                      4  cards max
    public int[] toolBox;       // TOKENS, TICKETS, MARKERS.    25 cards max
                                //                            = 100 total
}

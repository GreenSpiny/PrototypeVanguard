using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuItems
{
    static int cardCount = 0;
    static int nodeCount = 0;

    [MenuItem("Custom/Assign Card and Node names")]
    static void AssignNames()
    {
        cardCount = 0;
        nodeCount = 0;
        Scene scene = SceneManager.GetActiveScene();
        foreach (var obj in scene.GetRootGameObjects())
        {
            FindCardsRecursive(obj.transform);
            FindNodesRecursive(obj.transform);
        }
    }

    private static void FindCardsRecursive(Transform t)
    {
        int childCount = t.childCount;
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            if (child.GetComponent<Card>() != null)
            {
                child.name = "card_" + cardCount.ToString();
                cardCount++;
            }
            FindCardsRecursive(child);
        }
    }

    private static void FindNodesRecursive(Transform t)
    {
        int childCount = t.childCount;
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            Node node = child.GetComponent<Node>();
            if (node != null)
            {
                child.name = "node_" + System.Enum.GetName(typeof(Node.NodeType), node.Type) + '_' + nodeCount.ToString();
                nodeCount++;
            }
            FindNodesRecursive(child);
        }
    }

    [MenuItem("Custom/Remove Unused Assetbundle Names")]
    static void RemoveUnusedBundles()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();
    }
}

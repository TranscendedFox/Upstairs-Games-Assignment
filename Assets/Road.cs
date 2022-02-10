using UnityEngine;

public class Road : RoadEditorManager_Base
{
    public override bool Init()
    {
        RoadEditingLine = Instantiate(RoadEditingLineObj);
        RoadEditingLine.SetActive(false);

        originalColor = RoadEditingLine.GetComponentInChildren<Renderer>().material.GetColor("_Color");
        TargetPlacement = RoadEditingLine.gameObject.transform.GetChild(0).GetChild(0).GetComponent<Transform>();

        if (Junctions.Count == 0)
        {
            CreateJunction(new Vector3(250f, 0f, -200f));
        }        

        return true;
    }

    public override void StartRoadEdit()
    {
        StartedEditing = true;
        RoadEditingLine.SetActive(true);
    }

    public override void CreateJunction(Vector3 Position)
    {
        base.CreateJunction(Position);
    }

    public override void DrawEditingLine(Vector3 JunctionPosition, Vector3 MousePosition)
    {
        base.DrawEditingLine(JunctionPosition, MousePosition);
    }

    public override void AddSection(Transform From, Transform To)
    {
        base.AddSection(From,To);
    }

    public override bool CheckIfSectionExists(Transform A, Transform B)
    {
        return base.CheckIfSectionExists(A,B);
    }

    public override void DeleteCurrentJunction()
    {
        base.DeleteCurrentJunction();
    }    
}

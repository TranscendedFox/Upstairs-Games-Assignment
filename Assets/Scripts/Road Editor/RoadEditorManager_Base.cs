using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class RoadEditorManager_Base : MonoBehaviour
{
    public float MaxRoadDistance = 100f;
    public float MaxHeightDif = 3f;
    public float HeightCostAdd = 2f;
    protected List<GameObject> Junctions;
    protected List<GameObject> Sections;
    protected Transform CurrentJunction;
    protected GameObject RoadEditingLine;
    protected Transform TargetPlacement;
    protected bool StartedEditing;
    protected Color originalColor;

    [SerializeField] private Vector3 UIOffset;
    [SerializeField] private Text Cost;

    [SerializeField] protected GameObject JunctionObj;
    [SerializeField] protected GameObject SectionObj;
    [SerializeField] protected GameObject RoadEditingLineObj;

    private void Awake()
    {
        Junctions = new List<GameObject>();
        Sections = new List<GameObject>();
    }

    private void Update()
    {
        if (StartedEditing)
        {
            //Mouse ray to contact terrain
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Floor", "Junction"));
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (hit.collider.gameObject.layer == 10) // Floor
                {
                    CreateJunction(TargetPlacement.position);
                }
                else if (Vector3.Distance(hit.transform.parent.position, CurrentJunction.position) <= MaxRoadDistance) // Junction
                {
                    if (!CheckIfSectionExists(hit.transform.parent, CurrentJunction))
                    {
                        AddSection(hit.transform.parent, CurrentJunction);
                    }
                    else
                    {
                        Debug.Log("Section already exists");
                    }
                }
            }

            if (Input.GetMouseButton(1))
            {
                if (hit.collider.gameObject.layer == 11)
                {
                    CurrentJunction = hit.transform.parent;
                }
            }

            DrawEditingLine(CurrentJunction.position, hit.point);
        }


        // Debug Keys

        if (Input.GetKeyDown("p"))
        {
            Debug.Log(GetNumOfSections());
        }

        if (Input.GetKeyDown("k"))
        {
            DeleteCurrentJunction();
        }


    }

    public virtual void AddSection(Transform From, Transform To)
    {
        GameObject Section = Instantiate(SectionObj, To);
        Section.transform.localPosition = Vector3.zero;
        Section.transform.LookAt(From);
        Section.transform.localScale = RoadEditingLine.transform.localScale;
        Section.name = "Section " + From.position + " to " + To.position;
        Sections.Add(Section);
    }

    public virtual void CreateJunction(Vector3 Position)
    {

        //Checks for max road height
        if (Junctions.Count >= 1 && Mathf.Abs(Position.y - CurrentJunction.position.y) > MaxHeightDif)
        {
            Debug.Log("No Access");
            return;
        }

        GameObject a = Instantiate(JunctionObj, Position, gameObject.transform.rotation, gameObject.transform);
        a.name = "Junction " + Position;
        Junctions.Add(a);

        if (Junctions.Count > 1)
        {
            AddSection(CurrentJunction, Junctions[Junctions.Count - 1].transform);
        }

        CurrentJunction = a.transform;
        RoadEditingLine.transform.position = Position;
    }

    public virtual bool CheckIfSectionExists(Transform A, Transform B)
    {
        if (A.childCount != 0)
        {
            for (int i = 0; i < A.childCount; i++)
            {
                if (A.GetChild(i).name == "Section " + A.position + " to " + B.position ||
                A.GetChild(i).name == "Section " + B.position + " to " + A.position)
                {
                    return true;
                }
            }
        }

        if (B.childCount != 0)
        {
            for (int i = 0; i < B.childCount; i++)
            {
                if (B.GetChild(i).name == "Section " + A.position + " to " + B.position ||
                B.GetChild(i).name == "Section " + B.position + " to " + A.position)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public virtual void DrawEditingLine(Vector3 JunctionPosition, Vector3 MousePosition)
    {
        RoadEditingLine.transform.position = JunctionPosition;
        RoadEditingLine.transform.localScale = new Vector3(1, 1, Mathf.Min(Vector3.Distance(RoadEditingLine.transform.position, MousePosition), MaxRoadDistance));
        RoadEditingLine.transform.LookAt(MousePosition);

        //Ray from above to check height of terrain
        RaycastHit hit;
        Ray ray;
        Physics.Raycast(new Vector3(TargetPlacement.position.x, 100f, TargetPlacement.position.z), Vector3.down, out hit, Mathf.Infinity, 1 << 10);

        //Change color of Road Editing Line, red if road construction is unavailable
        if (Mathf.Abs(hit.point.y - CurrentJunction.position.y) > MaxHeightDif)
        {
            RoadEditingLine.GetComponentInChildren<Renderer>().material.SetColor("_Color", Color.red);
        }
        else
        {
            RoadEditingLine.GetComponentInChildren<Renderer>().material.SetColor("_Color", originalColor);
        }

        RoadEditingLine.transform.LookAt(hit.point);


        //calculate and view cost
        Cost.rectTransform.position = Input.mousePosition + UIOffset;

        float CostValue = Mathf.Ceil(Mathf.Floor((Mathf.Abs(hit.point.y - CurrentJunction.position.y)) * HeightCostAdd));
        Cost.text = CostValue.ToString();

    }

    public virtual void DeleteCurrentJunction()
    {
        if (Junctions.Count <= 1)
        {
            return;
        }

        for (int i = Sections.Count - 1; i >= 0; i--)
        {
            if (Sections[i].name.Contains(CurrentJunction.position.ToString()))
            {
                Destroy(Sections[i]);
                Sections.RemoveAt(i);
            }
        }

        Junctions.Remove(CurrentJunction.gameObject);
        Destroy(CurrentJunction.gameObject);
        CurrentJunction = Junctions[Junctions.Count - 1].transform;
    }

    public abstract bool Init();

    public abstract void StartRoadEdit();

    public int GetNumOfSections()
    {
        return Sections.Count;
    }    

}

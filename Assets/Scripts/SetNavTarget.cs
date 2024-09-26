using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;   //just for navmeshes and pathfinding
using UnityEngine.UI;

public class SetNavTarget : MonoBehaviour
{
    //SerializeField makes a box appear on the object this script is attached to, this allows you to drop in the declared item (e.g. GameObject, Camera etc.)
    //This allows you to edit what is being targeted quickly and without having to declare it within code
    [SerializeField]
    private TMP_Dropdown TargetDropdown;    //The dropdown menu for the target

    [SerializeField] 
    private Toggle lineToggle;

    [SerializeField]
    private List<Target> targetList = new List<Target>();   //A list of class "Target" objects

    private NavMeshPath navPath;    //The path being calculated

    private LineRenderer line;   //This renders a line which is then shown to the user

    //Vector3 holds the x, y and z of a game object (as well as things like the length which we do not need to know). Vector3.zero just sets them to 0,0,0
    readonly private Vector3 INVALIDPOSITION = new Vector3(0,-1000,0);  //Gives a position which should not be reachable by the user
    private Vector3 targetPosition = Vector3.zero;  //Holds the position of the current navigation target

    private bool showLine = false;  //Determines whether the line is being shown or not

    //Executes before anything in the Update section, used to initialise
    private void Start()
    {
        navPath = new NavMeshPath();
        line = transform.GetComponent<LineRenderer>();  //Just grabs the line renderer from the object this is attached to
        showLine = lineToggle.isOn;   //Ensures that the Line has expected default behavior by setting it to the showLine bool
    }

    // Update is called once per frame
    private void Update()
    {
        showLine = lineToggle.isOn; //Flips the boolean depending on whether the toggle is on/off

        //Checks user wants line to be shown and that the selected target is not on the same position as the user
        if (showLine && targetPosition != INVALIDPOSITION) 
        {
            //This asks for, in order: the position of the start of the path, the position of the end of the path, the area its should find a path in
            //and the actual path (where it should be stored)
            NavMesh.CalculatePath(transform.position,targetPosition, NavMesh.AllAreas, navPath);

            line.positionCount = navPath.corners.Length;
            line.SetPositions(navPath.corners);
            line.enabled = true;
        }
        else 
        {
            line.positionCount = 0; //Clears the line
        }

    }

    public void SetNewNavTarget()
    {
        targetPosition = INVALIDPOSITION;   //If the program cannot find a mathcing name, it defaults to using the invalid position which will auto clear the nav line

        //Finds the correct target in the list by comparing the name of the selected value and the name of the value within the list
        Target CurrentTarget = targetList.Find(newTarget => newTarget.targetName.Equals(TargetDropdown.options[TargetDropdown.value].text));

        if (CurrentTarget != null) 
        {
            targetPosition = CurrentTarget.targetObject.transform.position;
        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogManager : Singleton<LogManager>
{
    public bool isLogging = true;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void logOccupy(Vector2 index, GridCell cell)
    {
        if (isLogging)
        {

            Debug.Log($"{cell.type} occupy {index}");
        }
    }
    public void logRelease(Vector2 index, GridCell cell)
    {
        if (isLogging)
        {

            Debug.Log($"{cell.type} release {index}");
        }
    }
    public void logMove(Vector2 index, GridCell cell)
    {
        if (isLogging)
        {

            Debug.Log($"{cell.type} move to {index}");
        }
    }
    public void logGenerate(Vector2 index, string type)
    {
        if (isLogging)
        {

            Debug.Log($"{type} generated on {index}");
        }
    }
    

    public void logExchange(GridCell cell1, GridCell cell2)
    {
        if (isLogging)
        {

            Debug.Log($"{cell1.type} on {cell1.index} exchange with {cell2.type} on {cell2.index}");
        }
    }


    public void logCombine(Vector2 index, GridCell cell)
    {
        if (isLogging)
        {

            Debug.Log($"{cell.type} release {index}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

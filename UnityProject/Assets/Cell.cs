using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    /** Available area. Value 1 if cell is available, 0 otherwise. */
    public bool AA;
    /** Physarum mass. If starting point (one cell), PM = 100. */
    public float PM;
    /** CHemoAttractant. If nutrient source (more cells), CHA = 100. */
    public float CHA;
    /** Tube existence. */
    public bool TE;
    /** Type of the cell. */
    public CellType type;

    public Dir direction = Dir.NONE;
    public int age = 0;

    public Cell(bool aA, float pM, float cHA, bool tE, CellType type)
    {
        this.AA = aA;
        this.PM = pM;
        this.CHA = cHA;
        this.TE = tE;
        this.type = type;
    }

    public Cell(bool aA, float pM, float cHA, bool tE) : this(aA, pM, cHA, tE, CellType.A)
    { 
    }

    public Cell(CellType type) : this(false, 0, 0, false, type)
    {
    }

    public Cell() : this (false, 0,0, false, CellType.A)
    {
    }
}

public enum CellType
{
    S, N, U, A
};

public enum Dir
{
    N, NW, NE, E, W, S, SW, SE, NONE
};
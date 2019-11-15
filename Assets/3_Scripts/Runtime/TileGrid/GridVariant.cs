using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridVariant
{
    // Variant id
    public string variantID;
    // Geometry variants
    public GameObject[] geoVariants;
    // Material variants
    public MatVariant[] matVariants;

    // Set variant
    public void SetVariant(bool toOn)
    {
        if (geoVariants != null)
        {
            foreach (GameObject geoVar in geoVariants)
            {
                geoVar.SetActive(toOn);
            }
        }
        if (matVariants != null && toOn)
        {
            foreach (MatVariant matVar in matVariants)
            {
                if (matVar.renderers != null && matVar.materials != null)
                {
                    foreach (Renderer rend in matVar.renderers)
                    {
                        rend.sharedMaterials = matVar.materials;
                    }
                }
            }
        }
    }
}

[Serializable]
public class MatVariant
{
    // The renderers to be set to the desired material list
    public Renderer[] renderers;
    // The variant materials
    public Material[] materials;
}

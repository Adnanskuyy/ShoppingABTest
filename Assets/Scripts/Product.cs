using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour
{
    // A simple script to identify objects as products
    public enum ProductType { Cube, Sphere }

    [Header("Product Details")]
    public ProductType type;
    public string productName;
    public int price;
}


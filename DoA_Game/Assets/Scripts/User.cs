using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used For API Call
public class RootClass
{
    public int _total { get; set; }
    public List<User> users { get; set; }
}

public class User
{
    public string username { get; set; }
    public int points { get; set; }
}
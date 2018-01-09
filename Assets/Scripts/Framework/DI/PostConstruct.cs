using System;

[AttributeUsage(AttributeTargets.Method)]
public class PostConstruct : Attribute {

    public PostConstruct() { }

}

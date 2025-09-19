# A Todo list app in ASP.NET core In-Memory

## Overview  
This is an API based in memory Todo list app that will only track when it is running similer to a Redis database You just need the Microsoft ASP.Net core JWT auth NuGet package

> ### Further note on documentation:  
> Using the .http file, theres more notes in there to help set it up  

---

## GET /api/todoitems/  
You can add /[id] to get a specific todo item which either returns all todoitems or just the one with that id  

```json

[
  {
    "id": 1,
    "name": "string",
    "isCompleted": true
  }
]

```
---
## POST /api/todoitems/  
Add a todo item to the in-memory database  
Adding an item does need Authorization which you can get from another POST request, see {{PLACEHOLDER}}  
You can change the name to anything inside the quotes  
IsCompleted is bool value either true or false  
```json

{
  "name": "String",
  "IsCompleted": false
}

```
---
## DELETE /api/todoitems/{{id}}<br/>
Delete a todo item using the id from the in-memory database<br/>
Does require the Authorization header which you can get from another POST request, see {{PLACEHOLDER}}<br/>
---

## PUT /api/todoitems/{{id}}
Change a todo item from to complete to completed or vice versa

```json

{
  "id": {{id}},
  "name": "String",
  "IsCompleted": false
}

```
---

## POST /auth/login
**This is a mandatory to do anything usful on the api**  
the default user and passwords are as follows  
Admin to do all things  
Username: admin  
Password: admin123  
User to view  
Username: user  
Password: user123  

## Things to change
Add more restrictions for example all end points need Authorization  




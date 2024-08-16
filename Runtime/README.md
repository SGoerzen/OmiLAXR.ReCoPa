# Adapter Runtime Scripts

Place here your adapter scripts. We recommend to keep the folders and distribute your scripts in the folders.
When a folder has at least one script you can delete the `keep` file.

We also recommend to follow the namespace according the folder structure. 

@DEVELOPERS Please adjust `Install by using scoped registry` and `Install by using Git url` or remove them. Finally remove this line.

## OmiLAXR Pipeline

Each pipeline has an own actor. A pipeline is running through following steps:

1. Listeners: searching for target game objects
2. Filters: filter game objects
3. Tracking Behaviours: define when tracking is happening (async events)
4. Composers: create data statements triggered by events of tracking behaviours async events. On Demand Higher Composers can combine multiple statements to create further statements (e.g. semantic statements).
5. Hooks: Hooks between Composers and Endpoints. Hooks can manipulate existing statements.
6. Endpoints: Targets where statements are stored.

## OmiLAXR Components

You can implement own components. We recommend to copy files from `Examples` folder and adjust them.

- Actors: Pipeline Actor. The data statement will later have this actor.
- Composers: Compose interaction or scene data into analytics statements of your choice.
    - HigherComposers: Can react on created statements and compose new statements. For example, semantic statements can be generated if some specific statements were created.
- Endpoints: Handler which stores the statements.
- Extensions: C# Extension functions for objects
- Filters: They can filter detected objects. For example blacklists or specific components.
- Hooks: Manipulate or Discard existing statements.
- Listeners: Find candidates for pipelines (e.g. specific components or game objects).
- Pipelines: Each pipeline is isolated and has its own actor.
- TrackingBehaviours: Detect specific behaviour and offers events on that composers can react.


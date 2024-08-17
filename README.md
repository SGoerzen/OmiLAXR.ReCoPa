# OmiLAXR ReCoPa Adapter

Each folder has own `README.md` files with more instructions.

## Adapter dependencies
The adapter has following third-party dependencies:

- `com.pimdewitte.unitymainthreaddispatcher`: For dispatching main game thread from asynchronous socket io thread.
- ``

## Install by using scoped registry
1. Ensure in "Project settings" > "Package Manager" that you have the scoped registry with following settings:
    - Name: npmjs
    - URL: http://registry.npmjs.com
    - Scope(s): `com.rwth.unity.omilaxr.modules.recopa`
2. Go to Package Manager.
3. Click on the (+) button.
4. Select 'Add package by name'.
5. Place in 'Name' field: `com.rwth.unity.omilaxr.modules.recopa`.

### Adding scoped registry by using manifest.json (also recommended - quick way)
1. Alternatively, instead of adding the scoped registry inside Unity editor you can do it by using `manifest.json` file.
2. Go to you project root and then open `Packages/manifest.json`.
3. Ensure following entries in your file: `"scopedRegistries": [
   {
   "name": "npmjs",
   "url": "http://registry.npmjs.com/",
   "scopes": [
   "com.rwth.unity.omilaxr.modules.recopa"
   ]
   }]`.
4. By the way, you can also add here this package by adding `"com.rwth.unity.omilaxr.modules.recopa": "2.0.0"` to the dependencies (attention you can change the version).


## Install by using Git url
1. Go to Package Manager.
2. Click on the (+) button.
3. Select 'Add package from git URL'.
4. Paste `https://YOUR_REPOSITORY_URL.git` and confirm.

## For Developers

To work with this package we recommend to place it somewhere outside your Unity project (if the package gets an own git repository) or in root of your project.
Than, you can include the package into your project by going to `Window > Package Manager`, click on `(+)` button and finally import the `package.json` of this project by clicking on `Add package from disk`.

For production use we recommend to use `Add package form git URL` or using scoped registries (see below).

## Use another data format instead of xAPI
Remove references to `xAPI.Registry` and `OmiLAXR.xAPI`.

## Default Folder Structure

Here you can see the default structure of the adapter unity packages. The folders surrounding with (FOLDER) are not delivered by default.

- root
    - (Editor)
    - Examples
    - Plugins
    - Prefabs
    - Runtime
        - Actors
        - Composers
            - HigherComposers
        - Endpoints
        - Extensions
        - Filters
        - Hooks
        - Listeners
        - Pipelines
        - TrackingBehaviours
    - Tests
        - Runtime
        - (Editor)

## Use internal for project
If you do not wish to publish the package you can add the package in root of your Unity project. Accordingly you have to import your package for your project (see above "For Developers").

## Publication

You can publish your package at any npm registry.
It makes sense to publish packages for easier distribution in other projects.
But we recommend to use `npmjs.com`. [Here](https://docs.npmjs.com/creating-and-publishing-scoped-public-packages) you can get more details.
But the steps are very easy.

1. Create an account on `npmjs.com`.
2. On demand increase your `version` number in `package.json`.
3. Commit and push your changes.
4. Open a terminal.
5. Go to the root of your project.
6. Run `npm login` and login via browser (or what else you like).
7. Run `npm publish --access public`.
8. Wait until publication is ready.

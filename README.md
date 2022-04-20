# Motion Path Interpolation

**This is an SCP:SL Plugin for creating cinematic shots.**<br>

# How to use it:

1. **_Enable noclip_** (type `nc` in the game console [not RA] and press LeftAlt)
2. Create a motion path using `motionpath create`
3. Go to each desired point, and place it using `motionpath add`<br>
   (Optional) At each location, add camera rotation using `motionpath addrot`
4. Type `motionpath generate` in the Text-Based RA
5. Type `motionpath restart`
6. _**Enjoy the amazing shot you just created**_

_**IMPORTANT:**_ Since SCP:SL clamps the rotation between 0 and 360 degrees, you will have to manually input rotation
above 360° if there's a need to create a full 360° spin.

There's a bug where **the player's rotation will get locked** at a point in time. Currently, I am unable to counter this
problem. In case it occurs, export the path, rejoin the server and import it.

# Setup:

1. Install **[EXILED](https://github.com/Exiled-Team/EXILED/)** on
   your [dedicated server](https://en.scpslgame.com/index.php?title=Guide:Hosting_a_server)
2. Go to the **[releases page here](https://github.com/Axwabo/MotionPathInterpolation/releases/)**
   and download the **_MotionPathInterpolation.dll_** file
3. Place the DLL into your _EXILED Plugins_ folder: **_%appdata%\EXILED\Plugins_**
4. Restart the server if it's currently running
5. Get creative

# Configs:

| Name                | Default Value | Description                                              |
|---------------------|---------------|----------------------------------------------------------|
| is_enabled          | true          | If the plugin should be loaded.                          |
| max_points_per_user | 1000          | Maximum points a user can specify.                       |
| max_interval        | 300           | Maximum interval between points, used during generation. |
| default_easing      | Bezier        | Default easing for paths (Linear, Bezier or CubicSpline) |

# Command:

Use **motionpath** + _subcommand_ to manage your motion path. Aliases: `mpi`, `interpolation`, `motionpathinterpolation`

Required parameters are labeled with `<>`, optional parameters with `[]`

## Subcommand list:

| Name           |         Parameters         | Description                                                                                    |
|----------------|:--------------------------:|------------------------------------------------------------------------------------------------|
| create         | [interval: int]            | Adds a MotionPath component to the player.                                                     |
| delete         | -                          | Destroys the current MotionPath.                                                               |
| generate       | -                          | Generates the path using the specified points.                                                 |
| points         | -                          | Lists the specified points.                                                                    |
| rotations      | -                          | Lists the specified rotations.                                                                 |
| goToStart      | -                          | Teleports the player to the first position.                                                    |
| start          | -                          | Starts the motion (requires generated path).                                                   |
| pause          | -                          | Pauses the motion.                                                                             |
| stop           | -                          | Stops the motion and resets the timer.                                                         |
| restart        | -                          | Stops, then starts the motion after the delay, after teleporting the player to the start.      |
| interval       | [interval: int]            | Gets or sets the interval.                                                                     |
| delay          | [delay: int]               | Gets or sets the delay before starting the motion.                                             |
| easing         | [easing: EasingType]       | Gets or sets the easing mode that will be used to interpolate the path.                        |
| add            | [x: int] [y: int] [z: int] | Adds a new position to the list. If a coordinate is not specified, it uses that of the player. |
| addRot         | [y: int]                   | Adds a new rotation to the list. If the degree is not specified, it uses that of the player.   |
| removeFirst    | -                          | Removes the first position from the list, if there is one.                                     |
| removeLast     | -                          | Removes the last position from the list, if there is one.                                      |
| removeAt       | <index: int>               | Removes the position at the specified index, if there is one.                                  |
| clear          | -                          | Clears the positions from the list.                                                            |
| removeFirstRot | -                          | Removes the first rotation from the list, if there is one.                                     |
| removeLastRot  | -                          | Removes the last rotation from the list, if there is one.                                      |
| removeRotAt    | <index: int>               | Removes the position at the specified index, if there is one.                                  |
| clearRot       | -                          | Clears the rotations from the list.                                                            |
| generated      | -                          | Lists the generated points.                                                                    |
| generatedRot   | -                          | Lists the generated rotations.                                                                 |
| export         | -                          | Saves the current motion path into server memory with the user's name as the key.              |
| import         | -                          | Imports the motion path from server memory associated with the user's name, if it exists.      |

## Understanding interval:

The server runs on **60 TPS** (ticks per second) by default.

Path generation works like the following:<br>
Between each point, **_interval_** number of keyframes are generated.<br>
The server proceeds to the **next frame every tick.**<br>
This way, an **_interval of 120_** will be **_two times (2x) slower_** than the default 60.<br>
To make the motion faster, decrease the interval.
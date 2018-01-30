SDL2ThinLayer

by 1000101/ForsakenShell

Simple overview:

This is a threaded SDL library for anchoring the SDL_Window to a control or as a tool window on the window platform.

The thread SDL runs in should be the only thread that renders using SDL and can be done by registering for DrawScene events.

The thread will also send SDL_Events of importance in the intended scope of the library that are also run in the SDL thread.  The only exception to this is WindowClosed which will be invoked on the main form.


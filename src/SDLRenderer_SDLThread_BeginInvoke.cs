/*
 * SDLRenderer_SDLThread_BeginInvoke.cs
 *
 * These delegates and functions handle executing delegates in the SDL thread.  Certain
 * functions cannot be performed outside the thread that SDL is running in, such as drawing
 * and rendering.  If a function like this needs to be performed outside of the draw/event
 * mechanism, you can use SDLRenderer.BeginInvoke() or SDLRenderer.Invoke() to run the code
 * in the SDL thread.
 * 
 * NOTE:  These delegates will not execute immediately, they are handled via the event queue
 * in the SDL thread and should not be used for time critical code.
 * 
 * User: 1000101
 * Date: 1/30/2018
 * Time: 11:59 AM
 */
using System;
using System.Threading;
using System.Runtime.InteropServices;

using SDL2;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer
    {
        
        #region Custom SDL User Event IDs
        
        uint _sdlUEID_Invoke_NoParams;
        uint _sdlUEID_BeginInvoke_NoParams;
        
        #endregion
        
        #region Begin/Invoke structs passed as SDL_Event.user.data1
        
        struct UEInfo_Invoke_NoParams
        {
            public Client_Delegate_Invoke del;
            
            // Used by Invoke()
            public SemaphoreSlim sync;
            
            public bool IsBlocking
            {
                get
                {
                    return sync != null;
                }
            }
        }
        
        #endregion
        
        #region Public Invoke() and BeginInvoke()
        
        public void Invoke( Client_Delegate_Invoke del )
        {
            INTERNAL_SDLThread_PushInvokeEvent( del, _sdlUEID_Invoke_NoParams );
        }
        
        public void BeginInvoke( Client_Delegate_Invoke del )
        {
            INTERNAL_SDLThread_PushInvokeEvent( del, _sdlUEID_BeginInvoke_NoParams );
        }
        
        #endregion
        
        #region Internal SDL Thread Begin/Invoke
        
        #region Push Event
        
        void INTERNAL_SDLThread_PushInvokeEvent( Client_Delegate_Invoke del, uint userType )
        {
            // Invoke will cause a user event in the SDL thread.  Normal Invoke/BeginInvoke
            // cannot be used as the main loop for the SDL thread is always running.
            // Due to this being handled through the event system the call can be delayed.
            
            var sdlEvent = new SDL.SDL_Event();
            sdlEvent.type = (SDL.SDL_EventType)userType;
            
            // Begin/Invoke struct
            var ueInfo = new UEInfo_Invoke_NoParams();
            ueInfo.del = del;
            ueInfo.sync = null;
            
            if( userType == _sdlUEID_Invoke_NoParams )
            {
                // Create a semaphore with 1 slot and 0 available to handle Invoke()
                // This way we already hold the lone semaphore slot before we push
                // the event to the queue.  When the event is handled it will release
                // the semaphore count and allow this thread to resume execution.
                ueInfo.sync = new SemaphoreSlim( 0, 1 );
            }
            
            // Marshal it for SDL
            sdlEvent.user.data1 = INTERNAL_SDLThread_InvokeStructToPtr( ueInfo );
            
            // Now send the Begin/Invoke event to SDL
            if( SDL.SDL_PushEvent( ref sdlEvent ) != 1 )
                throw new Exception( "INTERNAL_SDLThread_PushInvokeEvent : SDL_PushEvent() failed!" );
            
            // Was this an Invoke?
            if( userType == _sdlUEID_Invoke_NoParams )
            {
                // Wait for the SDL thread to handle the Invoke()
                ueInfo.sync.Wait();
                ueInfo.sync.Release();
                
                // Dispose of the semaphore
                ueInfo.sync.Dispose();
                ueInfo.sync = null;
                
                // We need to free the unmanaged resources here
                INTERNAL_SDLThread_FreeInvokeStructPtr( ref sdlEvent.user.data1 );
                
            }
            
        }
        
        #endregion
        
        #region Marshalling
        
        UEInfo_Invoke_NoParams INTERNAL_SDLThread_PtrToInvokeStruct( IntPtr ueStruct )
        {
            return (UEInfo_Invoke_NoParams)Marshal.PtrToStructure( ueStruct, typeof( UEInfo_Invoke_NoParams ) );
        }
        
        IntPtr INTERNAL_SDLThread_InvokeStructToPtr( UEInfo_Invoke_NoParams ueInfo )
        {
            var ptr = Marshal.AllocHGlobal( Marshal.SizeOf( ueInfo ) );
            Marshal.StructureToPtr( ueInfo, ptr, false );
            return ptr;
        }
        
        void INTERNAL_SDLThread_FreeInvokeStructPtr( ref IntPtr ueStruct )
        {
            Marshal.DestroyStructure( ueStruct, typeof( UEInfo_Invoke_NoParams ) );
            Marshal.FreeHGlobal( ueStruct );
            ueStruct = IntPtr.Zero;
        }
        
        #endregion
        
        #region Begin/Invoke
        
        void INTERNAL_SDLThread_InvokeEvent( SDL.SDL_Event sdlEvent )
        {
            // Get the struct from the pointer
            var ueInfo = INTERNAL_SDLThread_PtrToInvokeStruct( sdlEvent.user.data1 );
            
            // Invoke the delegate
            if( ueInfo.del != null )
                ueInfo.del( this );
            
            if( ueInfo.IsBlocking )
            {
                // Signal the invoking thread that the delegate has been run.
                // The invoking thread will handle releasing the unmanaged resources.
                ueInfo.sync.Release();
            }
            else
            {
                // BeginInvoke() means we need to free the unmanaged resources
                INTERNAL_SDLThread_FreeInvokeStructPtr( ref sdlEvent.user.data1 );
            }
        }
        
        #endregion
        
        #endregion
        
    }
}

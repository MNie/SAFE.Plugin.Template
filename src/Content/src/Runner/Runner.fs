namespace SAFE.PluginName.Template

open SAFE
open SAFE.Core

#if (server)
    module Server =
        type PluginName ()
            inherit SAFEPlugin()
            interface ISAFEServerPlugin
#elif (client)
    module Client =
        type PluginName ()
            inherit SAFEPlugin()
            interface ISAFEClientPlugin
#elif (shared)
    module Shared =
        type PluginName ()
            inherit SAFEPlugin()
            interface ISAFESharedPlugin
#endif
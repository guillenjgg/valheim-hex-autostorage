# Valheim member access

- Always check Valheim source code or decompile the original game assembly to determine member accessibility. Do not infer access from publicized assemblies.
- The original game assembly is `G:/SteamLibrary/steamapps/common/Valheim/Valheim_Data/Managed/assembly_valheim.dll`.
- Publicized assemblies can compile direct calls that fail at runtime with `MethodAccessException`. Verify access in the original assembly before adding direct member access.

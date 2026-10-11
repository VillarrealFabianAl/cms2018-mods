"""Read-only metadata gate against the actual game's framework type definitions.

Requires dnfile; run after the net35 build. Does not load or execute game code.
"""
import sys
from pathlib import Path
import dnfile

def missing_types(plugin, managed):
    assembly = dnfile.dnPE(str(plugin))
    definitions = {}
    missing = []
    for reference in assembly.net.mdtables.TypeRef:
        scope = reference.ResolutionScope.row
        name = str(getattr(scope, 'Name', ''))
        if name not in ('mscorlib', 'System', 'System.Core'):
            continue
        if name not in definitions:
            runtime = dnfile.dnPE(str(managed / (name + '.dll')))
            definitions[name] = {(str(t.TypeNamespace), str(t.TypeName)) for t in runtime.net.mdtables.TypeDef}
            exported = getattr(runtime.net.mdtables, 'ExportedType', None)
            if exported:
                definitions[name].update((str(t.TypeNamespace), str(t.TypeName)) for t in exported)
        identity = (str(reference.TypeNamespace), str(reference.TypeName))
        if identity not in definitions[name]:
            missing.append(name + ':' + '.'.join(identity))
    return missing

if __name__ == '__main__':
    plugin = Path(sys.argv[1])
    managed = Path('C:/Program Files (x86)/Steam/steamapps/common/Car Mechanic Simulator 2018/cms2018_Data/Managed')
    missing = missing_types(plugin, managed)
    if missing:
        print('Unavailable runtime types:', ', '.join(missing))
        sys.exit(1)
    print('All directly referenced framework types exist in the installed game runtime.')

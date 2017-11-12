export VSINSTALLDIR="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional"
export VisualStudioVersion="15.0"

docfx -t ./_exported_templates/default ./docs/docfx.json --serve

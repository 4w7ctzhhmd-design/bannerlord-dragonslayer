"""Blender round-trip checks: object names, dimensions, materials, UVs and LODs."""
from pathlib import Path
import json
import bpy

root = Path(__file__).resolve().parent / "source"
report = json.loads((root / "asset-report.json").read_text())
for part in report["parts"]:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=str(root / (part["name"] + ".fbx")))
    expected = {part["name"], *part["lods"]}
    actual = {obj.name for obj in bpy.context.scene.objects}
    assert actual == expected, (actual, expected)
    obj = bpy.data.objects[part["name"]]
    assert all(abs(a-b) < 0.0001 for a, b in zip(obj.dimensions, part["dimensions"])), obj.dimensions
    assert obj.location.length < 0.0001, obj.location
    for name in expected:
        obj = bpy.data.objects[name]
        assert len(obj.data.uv_layers) > 0, name
        assert obj.data.materials and obj.data.materials[0].name.startswith("dragonslayer_iron"), name
        assert all(p.area > 1e-12 for p in obj.data.polygons), name
    print("PASS FBX round-trip:", part["name"])
print("PASS all source export contracts. Does not validate TaleWorlds import.")

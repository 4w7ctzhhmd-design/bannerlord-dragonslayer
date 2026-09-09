"""Original Dragonslayer-inspired geometry and deterministic PBR textures.

Run: blender --background --factory-startup --python assets/generate_sword.py
Outputs: .blend, four centred FBX crafting pieces with LODs, PNG textures, preview.
This is an export source; TaleWorlds editor import/client publish is still required.
"""
from pathlib import Path
import json
import math
import random

import bpy
import bmesh
from mathutils import Vector

OUT = Path(__file__).resolve().parent / "source"
OUT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0

# One atlas/material for every part: iron 0..0.60, edge 0.60..0.80, leather 0.80..1.
rng = random.Random(1989)
size = 512
textures = {}
for kind in ("albedo", "normal", "specular"):
    pixels = []
    for y in range(size):
        for x in range(size):
            u, v = x / size, y / size
            noise = rng.uniform(-0.023, 0.023)
            scratch = 0.07 if ((x * 37 + y // 21) % 167 == 0) else 0
            if u < 0.6:
                color = (0.105 + noise + scratch, 0.112 + noise + scratch, 0.12 + noise + scratch)
                metal, gloss = 0.95, 0.3 + noise
            elif u < 0.8:
                color = (0.52 + noise, 0.55 + noise, 0.58 + noise)
                metal, gloss = 1, 0.62 + noise
            else:
                weave = 0.02 * math.sin(x * 2.5) * math.sin(y * 2.5)
                color = (0.11 + noise + weave, 0.069 + noise + weave, 0.043 + noise + weave)
                metal, gloss = 0, 0.17
            rgba = (*color, 1) if kind == "albedo" else ((metal, gloss, 1, 0) if kind == "specular" else (0.5, 0.5, 1, 1))
            pixels.extend(rgba)
    im = bpy.data.images.new("dragonslayer_" + kind, width=size, height=size, alpha=True)
    im.colorspace_settings.name = "sRGB" if kind == "albedo" else "Non-Color"
    im.pixels.foreach_set(pixels)
    im.filepath_raw = str(OUT / (im.name + ".png"))
    im.file_format = "PNG"
    im.save()
    textures[kind] = im

mat = bpy.data.materials.new("dragonslayer_iron")
mat.use_nodes = True
nodes, links = mat.node_tree.nodes, mat.node_tree.links
bsdf = nodes.get("Principled BSDF")
albedo = nodes.new("ShaderNodeTexImage")
albedo.image = textures["albedo"]
links.new(albedo.outputs["Color"], bsdf.inputs["Base Color"])
packed = nodes.new("ShaderNodeTexImage")
packed.image = textures["specular"]
separate = nodes.new("ShaderNodeSeparateColor")
links.new(packed.outputs["Color"], separate.inputs["Color"])
links.new(separate.outputs["Red"], bsdf.inputs["Metallic"])
invert = nodes.new("ShaderNodeMath")
invert.operation = "SUBTRACT"
invert.inputs[0].default_value = 1
links.new(separate.outputs["Green"], invert.inputs[1])
links.new(invert.outputs[0], bsdf.inputs["Roughness"])


def finish(obj, zone=0, blade=False):
    """Consistent normals, triangulation, per-face planar UVs within atlas bands."""
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bmesh.ops.triangulate(bm, faces=bm.faces)
    bm.to_mesh(obj.data)
    bm.free()
    obj.data.update()
    obj.data.materials.clear()
    obj.data.materials.append(mat)
    uv = obj.data.uv_layers.new(name="UVMap")
    bounds = [(min(v.co[i] for v in obj.data.vertices), max(v.co[i] for v in obj.data.vertices)) for i in range(3)]
    for face in obj.data.polygons:
        # Broad flats retain dark iron, bevels and cutting surfaces use bright steel.
        fzone = (0 if abs(face.normal.y) > 0.97 else 1) if blade else zone
        low, high = [(0.02, 0.58), (0.62, 0.78), (0.82, 0.98)][fzone]
        dominant = max(range(3), key=lambda a: abs(face.normal[a]))
        axes = [a for a in range(3) if a != dominant]
        for loop_index in face.loop_indices:
            co = obj.data.vertices[obj.data.loops[loop_index].vertex_index].co
            p = [(co[a] - bounds[a][0]) / max(bounds[a][1] - bounds[a][0], 1e-6) for a in axes]
            uv.data[loop_index].uv = (low + p[0] * (high - low), 0.02 + p[1] * 0.96)
    obj.select_set(False)
    return obj


def box(name, dimensions, zone=0):
    bpy.ops.mesh.primitive_cube_add(size=1)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    return finish(obj, zone)


def make_blade():
    vertices, faces = [], []
    for z, width in [(-0.4945, 0.077), (-0.425, 0.105), (0.35, 0.098), (0.4945, 0.002)]:
        # Octagonal section: four broad surfaces and four bevel surfaces.
        for x, y in [(-1, 0), (-0.76, -1), (0.76, -1), (1, 0)]:
            vertices.append((x * width, y * 0.014, z))
        for x, y in [(0.76, 1), (-0.76, 1)]:
            vertices.append((x * width, y * 0.014, z))
    n = 6
    for ring in range(3):
        for i in range(n):
            a, b = ring*n+i, ring*n+(i+1)%n
            faces.append((a, b, b+n, a+n))
    faces += [tuple(reversed(range(n))), tuple(range(3*n, 4*n))]
    mesh = bpy.data.meshes.new("dragonslayer_blade_geometry")
    mesh.from_pydata(vertices, [], faces)
    obj = bpy.data.objects.new("dragonslayer_blade", mesh)
    scene.collection.objects.link(obj)
    return finish(obj, blade=True)


blade = make_blade()
guard = box("dragonslayer_guard", (0.23, 0.052, 0.0478))
bevel = guard.modifiers.new("Forged corners", "BEVEL")
bevel.width, bevel.segments = 0.006, 2
bpy.context.view_layer.objects.active = guard
bpy.ops.object.modifier_apply(modifier=bevel.name)
# Cylindrical handle plus actual helical binding geometry, not a smooth stick.
bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.023, depth=0.25)
grip = bpy.context.object
grip.name = "dragonslayer_grip"
finish(grip, 2)
wrap_vertices, wrap_faces = [], []
steps = 240
for i in range(steps + 1):
    t = i / steps
    angle = t * math.tau * 13
    for dz in (-0.007, 0.007):
        wrap_vertices.append((0.0245 * math.cos(angle), 0.0245 * math.sin(angle), -0.116 + 0.232*t + dz))
for i in range(steps):
    wrap_faces.append((2*i, 2*i+1, 2*i+3, 2*i+2))
mesh = bpy.data.meshes.new("wrap")
mesh.from_pydata(wrap_vertices, [], wrap_faces)
wrap = bpy.data.objects.new("binding", mesh)
scene.collection.objects.link(wrap)
finish(wrap, 2)
solid = wrap.modifiers.new("Leather thickness", "SOLIDIFY")
solid.thickness = 0.001
bpy.context.view_layer.objects.active = wrap
bpy.ops.object.modifier_apply(modifier=solid.name)
bpy.ops.object.select_all(action="DESELECT")
wrap.select_set(True)
grip.select_set(True)
bpy.context.view_layer.objects.active = grip
bpy.ops.object.join()
grip.select_set(False)
pommel = box("dragonslayer_pommel", (0.06, 0.05, 0.0544), 1)
parts = [blade, guard, grip, pommel]
report = {"units": "metres", "axis": "+Z blade, X width, Y thickness", "editorImported": False, "parts": []}
for obj in parts:
    # All crafting pieces must export centred at the world origin.
    bpy.ops.object.select_all(action="DESELECT")
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    lods = []
    for level, ratio in [(1, 0.5), (2, 0.22)] if obj == grip else [(1, 0.65)]:
        lod = obj.copy()
        lod.data = obj.data.copy()
        scene.collection.objects.link(lod)
        lod.name = obj.name + ".lod" + str(level)
        mod = lod.modifiers.new("LOD reduction", "DECIMATE")
        mod.ratio = ratio
        bpy.context.view_layer.objects.active = lod
        bpy.ops.object.modifier_apply(modifier=mod.name)
        lod.select_set(True)
        lods.append(lod)
    bpy.ops.export_scene.fbx(filepath=str(OUT / (obj.name + ".fbx")), use_selection=True,
                             object_types={"MESH"}, axis_forward="-Y", axis_up="Z",
                             apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS",
                             bake_anim=False, use_mesh_modifiers=True, add_leaf_bones=False,
                             path_mode="STRIP")
    report["parts"].append({"name": obj.name, "dimensions": list(obj.dimensions),
                             "triangles": sum(len(p.vertices)-2 for p in obj.data.polygons),
                             "lods": [lod.name for lod in lods]})
    for lod in lods:
        bpy.data.objects.remove(lod, do_unlink=True)
    obj.select_set(False)
(OUT / "asset-report.json").write_text(json.dumps(report, indent=2) + "\n")
# Save source with piece origins retained; assemble only for the preview render.
for im in textures.values():
    im.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(OUT / "dragonslayer.blend"))
blade.scale = (1.45, 1.45, 1.45)
blade.location.z = 0.15 + 0.035 + 0.4945 * 1.45
guard.location.z = 0.15 + 0.0239
grip.scale.z = 1.2
pommel.location.z = -0.15 - 0.0272
scene.render.engine = "CYCLES"
scene.cycles.samples = 32
scene.render.resolution_x, scene.render.resolution_y = 900, 1200
scene.render.resolution_percentage = 100
scene.world.color = (0.18, 0.18, 0.18)
scene.view_settings.view_transform = "AgX"
for location, power, area in [((1.8, -2.8, 3), 350, 3), ((-1.6, -0.8, 1), 180, 2), ((0.8, 2, 1.8), 400, 2)]:
    bpy.ops.object.light_add(type="AREA", location=location)
    light = bpy.context.object
    light.data.energy, light.data.shape, light.data.size = power, "DISK", area
    light.rotation_euler = (Vector((0, 0, 0.7)) - light.location).to_track_quat("-Z", "Y").to_euler()
bpy.ops.object.camera_add(location=(1.05, -3.8, 1.1))
camera = bpy.context.object
camera.rotation_euler = (Vector((0, 0, 0.72)) - camera.location).to_track_quat("-Z", "Y").to_euler()
camera.data.type, camera.data.ortho_scale = "ORTHO", 2.3
scene.camera = camera
scene.render.film_transparent = True
scene.render.filepath = str(OUT / "preview.png")
bpy.ops.render.render(write_still=True)
print("Exported original source FBXs, texture atlas, blend and preview; editor import NOT yet performed.")

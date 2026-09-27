"""Build native Godot animation tracks and a matching browser preview (no image edits)."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
# Regions include the magic/cape overhang; anchors keep the ground line fixed.
POSES = [
    {"name": "idle", "region": [0, 0, 512, 512], "anchor": [285, 480]},
    {"name": "windup", "region": [560, 0, 424, 512], "anchor": [215, 480]},
    {"name": "slash", "region": [992, 0, 544, 512], "anchor": [213, 475]},
    {"name": "cast", "region": [0, 512, 544, 512], "anchor": [295, 448]},
    {"name": "hurt", "region": [560, 512, 464, 512], "anchor": [245, 448]},
    {"name": "down", "region": [1024, 512, 512, 512], "anchor": [256, 458]},
]
# time, pose, x, y, scale_x, scale_y, rotation_degrees. Poses step; motion interpolates.
ANIMS = {
    "Idle": {"loop": True, "keys": [
        [0,0,0,0,1,1,-.35], [1,0,0,0,1.006,1.014,.35], [2,0,0,0,1,1,-.35]]},
    "Relaxed": {"loop": True, "keys": [
        [0,0,0,0,1,.985,-1], [1.4,0,0,0,1.005,1,0], [2.8,0,0,0,1,.985,-1]]},
    "Attack": {"loop": False, "keys": [
        [0,1,0,0,1,1,0], [.12,1,-12,0,.98,1.02,-4],
        [.22,2,34,0,1.04,.98,2], [.34,2,28,0,1.02,.99,1],
        [.46,0,6,0,1,1,0], [.6,0,0,0,1,1,0]]},
    "Cast": {"loop": False, "keys": [
        [0,0,0,0,1,1,0], [.12,3,-4,0,.99,1.01,-1],
        [.3,3,8,-4,1.025,1.025,1], [.52,3,4,-2,1.01,1.01,0],
        [.68,0,0,0,1,1,0], [.8,0,0,0,1,1,0]]},
    "Hit": {"loop": False, "keys": [
        [0,4,0,0,1,1,0], [.075,4,-15,0,.97,.98,-5],
        [.17,4,-8,0,1,1,-2], [.29,0,-2,0,1,1,0], [.4,0,0,0,1,1,0]]},
    "Dead": {"loop": False, "keys": [
        [0,4,0,0,1,1,0], [.22,4,-8,4,1,.94,-4],
        [.42,5,0,-5,1,1.02,0], [.58,5,0,0,1.025,.98,0],
        [.8,5,0,0,1,1,0], [1.2,5,0,0,1,1,0]]},
    "Revive": {"loop": False, "keys": [
        [0,5,0,0,1,1,0], [.18,5,0,-4,1,1.02,0],
        [.32,4,0,0,1,.96,-2], [.5,0,0,-4,1,1.02,0], [.7,0,0,0,1,1,0]]},
}


def vec(kind, values):
    return f"{kind}({', '.join(str(round(v, 7)) for v in values)})"


def build_library():
    import math
    out = ['[gd_resource type="AnimationLibrary" load_steps=8 format=3]', '']
    for name, spec in ANIMS.items():
        keys = spec["keys"]
        out += [f'[sub_resource type="Animation" id="Animation_{name}"]',
                f'resource_name = "{name}"', f'length = {keys[-1][0]}',
                f'loop_mode = {1 if spec["loop"] else 0}']
        tracks = [
            ("Visuals/Sprite:region_rect", 1, [vec("Rect2", POSES[k[1]]["region"]) for k in keys]),
            ("Visuals/Sprite:offset", 1, [vec("Vector2", [-n for n in POSES[k[1]]["anchor"]]) for k in keys]),
            ("Visuals:position", 0, [vec("Vector2", k[2:4]) for k in keys]),
            ("Visuals:scale", 0, [vec("Vector2", k[4:6]) for k in keys]),
            ("Visuals:rotation", 0, [str(round(math.radians(k[6]), 7)) for k in keys]),
        ]
        for i, (path, update, values) in enumerate(tracks):
            out += [f'tracks/{i}/type = "value"', f'tracks/{i}/path = NodePath("{path}")',
                    f'tracks/{i}/interp = 1', f'tracks/{i}/enabled = true',
                    f'tracks/{i}/keys = {{',
                    '"times": PackedFloat32Array(' + ', '.join(str(k[0]) for k in keys) + '),',
                    '"transitions": PackedFloat32Array(' + ', '.join('1' for _ in keys) + '),',
                    f'"update": {update},', '"values": [' + ', '.join(values) + ']', '}']
        out += ['']
    out += ['[resource]', '_data = {', ',\n'.join(
        f'&"{name}": SubResource("Animation_{name}")' for name in ANIMS), '}']
    return '\n'.join(out) + '\n'


def build_preview():
    data = json.dumps({"poses": POSES, "animations": ANIMS})
    return '''<!doctype html><html lang="en"><meta charset="utf-8">
<title>Isekai Hero · Combat animations</title>
<style>
body{margin:0;background:#121923;color:#edf4f2;font:16px system-ui}main{max-width:1060px;margin:36px auto;padding:0 24px}
h1{font-size:30px}p{color:#a9bac6;line-height:1.6}button,select{font:inherit;padding:10px 18px;margin:4px;border:1px solid #526077;border-radius:8px;background:#243145;color:inherit;cursor:pointer}
button[aria-pressed=true]{background:#623989;border-color:#c48eff}canvas{display:block;width:100%;border:1px solid #354255;border-radius:12px;margin:20px 0}
label{margin-left:16px}small{color:#a9bac6}
</style><main><h1>Isekai Hero</h1><p>Office worker. System reader. Reluctant adventurer.<br>
Original combat art with native Godot animation timing. Select an action to preview it; defeat holds until revival or another action.</p>
<div id="buttons"></div><label>Backdrop <select id="backdrop"><option value="dungeon">Dungeon</option><option value="light">Light</option><option value="grid">Transparency grid</option></select></label>
<label><input id="slow" type="checkbox"> Quarter speed</label><button id="pause">Pause</button>
<canvas id="view" width="1060" height="520" aria-label="Animated character preview"></canvas>
<small id="status"></small><p>Seven animations: idle, relaxed, attack, cast, hit, defeat and revival. Six illustrated poses with interpolated motion; this is a pose-based animation set, not a skeletal rig.</p></main>
<script>const data=DATA;
const canvas=document.querySelector('canvas'), ctx=canvas.getContext('2d'), img=new Image();
img.src='../../IsekaiHero/images/character/hero_combat_atlas.png';
let current='Idle',time=0,last=0,paused=false;
function select(name){current=name;time=0;document.querySelectorAll('[data-animation]').forEach(b=>b.setAttribute('aria-pressed',b.dataset.animation===name));}
for(const name of Object.keys(data.animations)){const b=document.createElement('button');b.textContent=name;b.dataset.animation=name;b.onclick=()=>select(name);document.querySelector('#buttons').append(b);}select('Idle');
document.querySelector('#pause').onclick=e=>{paused=!paused;e.target.textContent=paused?'Resume':'Pause'};
function render(now){let dt=last?(now-last)/1000:0;last=now;if(!paused)time+=Math.min(dt,.1)*(document.querySelector('#slow').checked?.25:1);
let anim=data.animations[current],keys=anim.keys,end=keys.at(-1)[0];
if(time>end){if(anim.loop)time%=end;else if(current!=='Dead'){select('Idle');anim=data.animations[current];keys=anim.keys;}else time=end;}
let a=keys[0],b=a;for(let i=0;i<keys.length;i++){if(keys[i][0]<=time)a=keys[i];if(keys[i][0]>time){b=keys[i];break;}b=a;}
const f=b[0]===a[0]?0:(time-a[0])/(b[0]-a[0]),v=a.map((x,i)=>i<2?x:x+(b[i]-x)*f);
const bg=document.querySelector('#backdrop').value;ctx.fillStyle=bg==='light'?'#dce6e5':'#1b2831';ctx.fillRect(0,0,1060,520);
if(bg==='grid'){for(let y=0;y<520;y+=24)for(let x=0;x<1060;x+=24){ctx.fillStyle=(x/24+y/24)%2?'#56616c':'#303c47';ctx.fillRect(x,y,24,24);}}
ctx.strokeStyle=bg==='light'?'#9fadae':'#435964';ctx.beginPath();ctx.moveTo(80,440);ctx.lineTo(980,440);ctx.stroke();
ctx.save();ctx.translate(520+v[2],440+v[3]);ctx.rotate(v[6]*Math.PI/180);ctx.scale(v[4]*.65,v[5]*.65);
const pose=data.poses[a[1]],r=pose.region,anchor=pose.anchor;if(img.complete&&img.naturalWidth)ctx.drawImage(img,...r,-anchor[0],-anchor[1],r[2],r[3]);ctx.restore();
document.querySelector('#status').textContent=current+' · '+time.toFixed(2)+'s · '+data.poses[a[1]].name;requestAnimationFrame(render);}
img.onload=()=>requestAnimationFrame(render);img.onerror=()=>document.querySelector('#status').textContent='Atlas could not load. Open this preview from the repository.';
</script></html>'''.replace('DATA', data)


if __name__ == '__main__':
    (ROOT / 'IsekaiHero/animations/hero_combat.tres').write_text(build_library(), encoding='utf-8')
    (ROOT / 'docs/animations/preview.html').write_text(build_preview(), encoding='utf-8')
    print('Built seven Godot animations and the matching interactive preview.')

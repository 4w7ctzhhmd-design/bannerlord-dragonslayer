using System;
using DragonslayerInstantKill;

static bool Hit(string item = "dragonslayer", bool human = true, bool active = true,
    bool attacker = true, bool strike = true, bool shield = false, bool alternate = false,
    bool missile = false, bool fall = false, bool charge = false) =>
    HitPolicy.ShouldKill(item, human, active, attacker, strike, shield, alternate, missile, fall, charge);
static void Check(string name, bool value) { if (!value) throw new Exception(name); }
Check("Blade strike (no minimum damage condition)", Hit());
Check("Other weapon", !Hit(item: "native_sword"));
Check("Empty weapon", !Hit(item: null));
Check("Mount", !Hit(human: false));
Check("Dead/removed agent", !Hit(active: false));
Check("Self or missing attacker", !Hit(attacker: false));
Check("Parry/world contact", !Hit(strike: false));
Check("Shield contact", !Hit(shield: true));
Check("Kick/bash", !Hit(alternate: true));
Check("Missile", !Hit(missile: true));
Check("Fall", !Hit(fall: true));
Check("Horse charge", !Hit(charge: true));
Console.WriteLine("PASS: 12 instant-kill selection cases; no engine/gameplay execution.");

from __future__ import annotations

import hashlib
import json
import re
import shutil
import subprocess
from pathlib import Path


ROOT = Path(__file__).resolve().parent
SOURCE_REVISION = "bc19213"
DIALOGUE_PATH = Path("Assets/MonoBehaviour/AFTERLIVES Dialogue Database.asset")
SCENE_PATHS = (
    Path("Assets/Scenes/MainMenu.unity"),
    Path("Assets/Scenes/SampleScene.unity"),
)
BACKUP_DIR = ROOT / "_TextOverhaulBackup"


# Ordered by the 257 unique, non-empty Dialogue Text values in SOURCE_REVISION.
FINAL_TRANSLATIONS = [
    "Zaključano je.",  # 001
    "Odmakni se.",  # 002
    "NASTAVI",  # 003
    "Telefon zvoni.\n\nMaddie. Nisi očekivao da će nazvati.",  # 004
    "JAVI SE",  # 005
    '"Koji se vrag događa?"',  # 006
    "Nemam pojma.",  # 007
    "Jesi li dobro?",  # 008
    '"Jesam. Dobro sam. Ne moraš se brinuti za mene."',  # 009
    "Jesi li ozlijeđena?",  # 010
    "Doći ću do tebe.",  # 011
    '"Mislim da nisam. Barem me ništa ne boli."',  # 012
    '"Ne! Nemoj dolaziti.\n\nStvarno sam dobro. Samo sam htjela provjeriti znaš li što se događa."',  # 013
    "Jesi li sigurna?",  # 014
    "Trebao bih ipak doći.",  # 015
    '"Jesam. Ne brini se."',  # 016
    '"...dobro. Ali mislim da most prema Bloku 29 više nije otvoren."',  # 017
    "Pokušat ću doći.",  # 018
    '"Od eksplozije mi još zvoni u ušima."',  # 019
    "Fotografija kina u koje je Maddie često išla.\n\nObožava filmove.",  # 020
    "Tvoj telefon.",  # 021
    "Kratka bilješka tvog susjeda.",  # 022
    "PROČITAJ",  # 023
    '"Maddie,\n\nzaštitit ću te."',  # 024
    "Neoprani tanjur. Susjed je očito otišao na brzinu.",  # 025
    "Nadzorničin interfon.",  # 026
    "Halo?",  # 027
    'Interfon kratko zazuje. Zatim se javi umoran glas stare nadzornice.\n\n"Ima li koga?"',  # 028
    "Ja sam, susjed.",  # 029
    "Moram preko mosta.",  # 030
    'Duboko uzdahne.\n\n"Zašto još nisi otišao, sine? Rekli su nam da se evakuiramo. Onaj čovjek s jednim okom već je otišao."',  # 031
    "Moram pronaći Maddie.",  # 032
    '"Maddie?\n\nAh, Maddie. Moj unuk stalno govori o njoj.\n\nA sad ni on neće otići. Budala jedna."',  # 033
    '"Preko mosta? Ne možeš, sine. Žao mi je. Vojnici su naredili da ga zaključam.\n\nŠto ćeš uopće tamo?"',  # 034
    "Zašto on još nije otišao?",  # 035
    "Moram preko tog mosta.",  # 036
    '"Ne znam!" vikne, glasnije nego što je htjela.\n\n"Zaključao se u stan. Bog zna što radi unutra."',  # 037
    '"Znam, sine. Ali vojnici te neće pustiti. Ako ti otvorim most, vjerojatno će me ubiti."',  # 038
    'Nadzornica zastane.\n\n"...stvarno? Ako ga izvučeš, mogla bih ti nakratko otvoriti most. Samo tebi. Možda vojnici neće primijetiti.\n\nAli moraš nagovoriti mog unuka da ode."',  # 039
    "Mogu izvući vašeg unuka. Vi mi otvorite most.",  # 040
    "Dogovoreno.",  # 041
    '"Hvala ti. Od srca.\n\nOtključat ću vrata njegova stana. I molim te, čuvaj se."',  # 042
    "...",  # 043
    '"Više stvarno ne znam što mu je u glavi."\n\nNadzornica prekine vezu.',  # 044
    '"A ja tu ništa ne mogu", kaže.',  # 045
    "Što govori o Maddie?",  # 046
    '"Ma... svašta."',  # 047
    '"Tamo se ide samo dublje u zgradu. Rekli su nam da se evakuiramo. Onaj čovjek s jednim okom već je otišao."',  # 048
    "Nitko se ne javlja.",  # 049
    "Razbijeno ogledalo.",  # 050
    "UMETNI KRHOTINU",  # 051
    "Ogledalo je ponovno cijelo.\n\nPri dnu je nešto urezano.",  # 052
    '"Ispravio sam sve što na meni nije valjalo.\n\nSad će me voljeti."',  # 053
    "ČITAJ DALJE",  # 054
    '"Sad će me Maddie voljeti."',  # 055
    "Tipkovnica za unos šifre.",  # 056
    "PREGLEDAJ",  # 057
    'Na zaslonu piše: "Pristup mostu Bloka 29".\n\nNe znaš šifru.',  # 058
    "Na prašnjavo ogledalo pribijena je bilješka.",  # 059
    '"Na vratu, s desne strane, imam ružnu mrlju.\n\nOči su mi odvratne.\n\nNi koža mi ne izgleda kako treba."',  # 060
    '"Odvijač je bio dovoljno oštar.\n\nMrlje više nema."',  # 061
    "Odvijač leži pokraj hrpice mesa.",  # 062
    "KRHOTINA OGLEDALA (1/3).",  # 063
    "UZMI",  # 064
    "Nešto je zapelo u zahodu.",  # 065
    "Mala bilježnica.",  # 066
    '"Popravljeno:\n\n- zubi\n- obrve\n- nokti\n- koža"',  # 067
    '"Još moram popraviti:\n\n- oči\n\nOči su mi odvratne."',  # 068
    "Cijevi vode u kupaonicu kat niže.\n\nNa nosaču u sredini nedostaje ventil.",  # 069
    "POSTAVI VENTIL",  # 070
    "Ljudska koža.",  # 071
    "Mali lokot sa znakom oka.",  # 072
    "OTKLJUČAJ KLJUČEM OKA",  # 073
    "KLJUČ OKA (3 uporabe).",  # 074
    "Bilješka.",  # 075
    '"Hodnici su prazni.\n\nZaglavio sam ovdje, među svim tim ogledalima."',  # 076
    '"Od sada gledam samo u razbijena."',  # 077
    "Dva okretna ležišta.\n\nU njih bi stala dva mala, okrugla predmeta.",  # 078
    "UMETNI OČI",  # 079
    "Bilješka je pribijena na stražnjoj strani ormara.",  # 080
    '"Pokušat ću ponovno sutra.\n\nNe mogu pronaći pilu."',  # 081
    "VENTIL ZA CIJEV.",  # 082
    "Nešto je na donjoj polici.",  # 083
    "Netko ti je gurnuo bilješku ispod vrata.",  # 084
    '"STANARIMA BLOKA 28\n\nIzvanredno stanje.\n\nOdmah napustite grad."',  # 085
    '"Blok 28 - Stan 256"',  # 086
    '"Blok 28 - Stan 254"',  # 087
    '"Blok 28 - Stan 255"',  # 088
    "Malo ogledalo s pričvršćenom bilješkom.",  # 089
    '"Znam šifru za most.\n\nTi pomozi meni, ja ću tebi."',  # 090
    '"Molim te, sastavi moje posebno ogledalo.\n\nEksplozija ga je srušila sa stola i razbila.\n\nBez njega ne znam što ću."',  # 091
    "KRHOTINA OGLEDALA (2/3).",  # 092
    '"Riješio sam se onih groznih očiju.\n\nVrijedit će svake boli."',  # 093
    "PAR LJUDSKIH OČIJU.",  # 094
    "Dva ležišta. Na svakome je ljudsko oko.",  # 095
    "OKRENI LIJEVO OKO",  # 096
    "OKRENI DESNO OKO",  # 097
    "KRHOTINA OGLEDALA (3/3).",  # 098
    '"Kao što sam obećao:\n\nšifra mosta je\n2-2-0-0-0."',  # 099
    "Mala knjiga pjesama koje si napisao o Maddie.",  # 100
    'Naslov: "Moja toplina u Hladnom gradu".\n\nNikad joj nisi pokazao nijednu. Kad ih zapišeš, sve između vas djeluje tako stvarno.',  # 101
    "Kutija cigareta.\n\nTi ne pušiš. Maddie puši.",  # 102
    "Plakat starog ratnog filma: jednonogi junak spašava djevojku.\n\nMaddie ga obožava.",  # 103
    '"Blok 28 - Stan 255\n\nStražnji ulaz"',  # 104
    "Telefon zvoni. Maddie.",  # 105
    '"Opet ti kažem, ne moraš dolaziti.\n\nStvarno sam dobro."',  # 106
    "Svejedno dolazim.",  # 107
    '"Molim te, misli na sebe."',  # 108
    "Ne brini se.",  # 109
    "Važnije mi je da si ti sigurna.",  # 110
    '"Dobro... ako baš inzistiraš.\n\nAli nemoj ovo shvatiti kao nešto više."',  # 111
    "Uskoro sam tamo.",  # 112
    "Čuvaj se.",  # 113
    '"Dobro."',  # 114
    "Volim te.",  # 115
    "Maddie je već prekinula.\n\nNije te čula.",  # 116
    '"Mislim da sam već na sigurnom."',  # 117
    "Molim te. Pusti me da dođem.",  # 118
    "Ne zvučiš baš sigurno.",  # 119
    '"Pa... ne znam što da ti kažem."',  # 120
    '"...dobro.\n\nSamo nemoj ovo krivo shvatiti."',  # 121
    "Neću.",  # 122
    "Razumijem.",  # 123
    '"...možeš doći.\n\nAli samo da se razumijemo: ovo nije ništa više od toga."',  # 124
    '"U redu."',  # 125
    'Na zaslonu piše: "Pristup mostu Bloka 29".',  # 126
    "UNESI ŠIFRU:\n2-2-0-0-0",  # 127
    '"..."',  # 128
    "Kako da dođem do Bloka 29?",  # 129
    '"...sve ove godine... i što sam dobio zauzvrat?..."',  # 130
    "Treba li ti pomoć?",  # 131
    '"...stotine... ne, tisuće mrtvih..."',  # 132
    '"...a gdje je moja nagrada?..."',  # 133
    '"...boli..."',  # 134
    '"...u bunkeru lijevo ima tableta.\n\n...donesi ih i odvest ću te do Bloka 29..."',  # 135
    "Moram do Bloka 29.",  # 136
    "Pilot ti pruža KLJUČ BUNKERA.",  # 137
    '"...mislio sam da će ona..."',  # 138
    "OTKLJUČAJ BUNKER",  # 139
    "Vrata su zakovana daskama.\n\nKljučanica je neobično velika.",  # 140
    "POGLEDAJ KROZ KLJUČANICU",  # 141
    "Na stolu je bočica tableta.",  # 142
    "ODMAKNI SE",  # 143
    "PREPILI DASKE",  # 144
    "Pilotova bilješka.",  # 145
    '"Ona me treba."',  # 146
    '"Za nju bih učinio sve."',  # 147
    '"Zapravo, nemoj dolaziti.\n\nMolim te. Nemoj."',  # 148
    "Zašto?",  # 149
    "Kako to misliš?",  # 150
    '"Nazvala sam samo zbog bombe.\n\nNemoj iz toga izvlačiti nešto čega nema."',  # 151
    '"Samo sam se zabrinula za tebe.\n\nNisam te trebala zvati. Sad je ispalo kao da sam ti nešto obećala."',  # 152
    "Nisi ti kriva.",  # 153
    "Jesi li barem na sigurnom?",  # 154
    "Moram te vidjeti.",  # 155
    '"Nije važno.\n\nMolim te, okreni se i idi. Ostavi me na miru."',  # 156
    '"Jesam, sigurna sam.\n\nSad se okreni i idi. Molim te, ostavi me na miru."',  # 157
    '"Ostavi me na miru. Samo idi, molim te."',  # 158
    "Maddie?",  # 159
    "Prekinula je.\n\nSvejedno moraš do nje.",  # 160
    "SAČMARICA.\n\nNe znaš pucati iz nje.\nNisi poput pilota.",  # 161
    "PILA.",  # 162
    "Vrata su zakovana daskama.",  # 163
    "Nadzorni sustav.",  # 164
    "Monitori prikazuju cijeli Maddiein stan. Snimke dolaze iz nekoliko skrivenih kamera.",  # 165
    "DNEVNI BORAVAK",  # 166
    "KUHINJA",  # 167
    "SPAVAĆA SOBA",  # 168
    "Dnevni boravak je neuredan. Sve izgleda kao da će se raspasti.\n\nNa stolu je kutija cigareta.",  # 169
    "Obična, skučena kuhinja.\n\nNa pultu se gomilaju prljavi tanjuri.",  # 170
    "Maddie leži na kauču i gleda televiziju.\n\nKao i uvijek, savršena.",  # 171
    '"Ja je štitim. Imam pravo znati što radi.\n\nLijep je osjećaj biti joj ovako blizu."',  # 172
    '"Kad su me poslali braniti Blok 29, nisam mogao vjerovati koliko imam sreće.\n\nNitko neće stati između nas. Dok sam ja ovdje, sigurna je."',  # 173
    "Bočica tableta.\n\nOdnesi je pilotu.",  # 174
    "Ne odlazi iz bunkera dok ne uzmeš tablete za pilota.",  # 175
    "DAJ MU TABLETE",  # 176
    "Pilot proguta tablete.",  # 177
    '"...hvala..."',  # 178
    "Odvedi me do Bloka 29.",  # 179
    '"...žao mi je. Ne mogu.\n\nNemam ključ. Više nemam ništa."',  # 180
    '"A i da mogu, ne bih te pustio. Tko zna što bi joj učinio..."',  # 181
    "O čemu ti pričaš?",  # 182
    '"Žao mi je...\n\nNišta od ovoga nisam zaslužio..."',  # 183
    "Lagao si mi.",  # 184
    '"...ne prilazi Maddie..."',  # 185
    '"...zbog te djevojke ostao sam bez noge."',  # 186
    "Pilot se više ne javlja.\n\nViše ti ne treba.",  # 187
    '"A što si ti izgubio?"',  # 188
    '"Blok 29 - Dizala"',  # 189
    "NAZOVI MADDIE",  # 190
    '"Ne dolazi."',  # 191
    "Zašto?",  # 192
    "Ne mogu.",  # 193
    '"Nazvala sam jer je pala bomba. Rat je. Htjela sam znati jesi li živ.\n\nSamo zato sam te nazvala."',  # 194
    '"Zašto ti baš sve mora nešto značiti?\n\nUvijek od svega napraviš priču koje nema."',  # 195
    '"Ja te uopće ne poznajem."',  # 196
    '"Plašiš me. Ne prilazi mi."',  # 197
    "Volim te.",  # 198
    "Trebam te.",  # 199
    "Dolazim do tebe.",  # 200
    '"Nisam htjela da ovako završi.\n\nNisam te trebala zvati."',  # 201
    "Maddie je prekinula.\n\nPrešao si toliki put. Ne možeš sada odustati.",  # 202
    "Najprije nazovi Maddie.",  # 203
    '"Blok 29 - Stan 471"',  # 204
    '"Blok 29 - Stan 472"',  # 205
    '"Blok 29 - Stan 473"',  # 206
    '"Blok 29 - Stan 474"',  # 207
    '"Blok 29 - Stan 475"',  # 208
    '"Blok 29 - Stan 476\n\nMaddie"',  # 209
    "Maddie je malo odškrinula vrata.\n\nNe vidiš je, ali unutra je.",  # 210
    "Maddie, ja sam.",  # 211
    '"Izlazi iz mog stana. Molim te. Samo me ostavi na miru."',  # 212
    '"Što hoćeš od mene?\n\nŠto još hoćeš?"',  # 213
    "Došao sam te zaštititi.",  # 214
    '"Zaštititi? Od čega?\n\nSigurna sam. Pilot se pobrinuo za to."',  # 215
    '"Izgubio je nogu da mene spasi.\n\nTo je hrabrost, zar ne?"',  # 216
    '"...što ja uopće govorim?\n\nOstavi me na miru. Idi."',  # 217
    '"...zbog mene je ostao bez noge..."',  # 218
    '"...zbog mene je ostao bez noge...\n\n...bio je tako hrabar..."',  # 219
    "Prešao si toliki put. Sada nema odustajanja.",  # 220
    "Sjetiš se PILE iz bunkera.\n\nI ti možeš biti hrabar.",  # 221
    "Možeš biti hrabar.\n\nHrabriji od njega.\n\nMaddie će te napokon voljeti.",  # 222
    "PREPILI NOGU",  # 223
    "Nema odustajanja.\n\nAko budeš hrabar, Maddie će te voljeti.",  # 224
    '"Što si si napravio?\n\nŠto nije u redu s tobom?"',  # 225
    "Vidiš? Hrabar sam.",  # 226
    "Sve bih učinio za tebe.",  # 227
    '"Makni se od mene!\n\nGubi se iz mog stana!"',  # 228
    "Maddie tiho plače.",  # 229
    '"O, Bože..."',  # 230
    '"Molim te... idi."',  # 231
    "Bit ću savršen.",  # 232
    '"...o čemu ti to govoriš?"',  # 233
    "UZMI ODVIJAČE",  # 234
    "Odvijači su dovoljno oštri.\n\nNakon ovoga bit ćeš savršen.",  # 235
    "ZABODI ODVIJAČ U DESNO OKO",  # 236
    "ZABODI ODVIJAČ U LIJEVO OKO",  # 237
    "Boli.\n\nMaddie će te zato voljeti.",  # 238
    '"Prestani...\nMolim te, prestani."',  # 239
    '"Prestani...\nMolim te, prestani."',  # 240
    '"Bože, molim te, prestani!"',  # 241
    '"Učinit ću što god hoćeš.\nSamo prestani. Molim te!"',  # 242
    '"...volim te! Dobro?\n\nVolim te, samo prestani..."',  # 243
    "U ormariću je bilješka.",  # 244
    '"Sve je staro, slomljeno i curi.\n\nObećali su poslati nekoga, ali nitko nikad nije došao."',  # 245
    "Maketa borbenog aviona.",  # 246
    "Prazan umivaonik.",  # 247
    "Kutija Maddieinih cigareta.",  # 248
    "Vrata Maddieine spavaće sobe.\n\nZaključana su.",  # 249
    "Veliko ogledalo.\n\nPred njim su dva odvijača.",  # 250
    "POGLEDAJ SE U OGLEDALO",  # 251
    "Te nedostatke još možeš ispraviti.\n\nMožeš biti savršen. Onda će te Maddie morati voljeti.\n\nSamo joj pokaži.",  # 252
    "Oči su ti odvratne.",  # 253
    "UZMI ODVIJAČE",  # 254
    "U ogledalu je tvoje lice.\n\nUmorno, ali blago. Odlučno.\n\nZašto te Maddie ne može voljeti?",  # 255
    "PRIBLIŽI SE",  # 256
    "...oči. Mora da su problem oči.\n\nMutne su, krvave i slabe.",  # 257
]


GENDER_CONTEXT_GUARDS = {
    "male protagonist": {
        4: "očekivao",
        15: "Trebao bih",
        100: "napisao",
        194: "jesi li živ",
        202: "Prešao si",
        220: "Prešao si",
        221: "hrabar",
        222: "Hrabriji",
        224: "hrabar",
        226: "Hrabar sam",
        227: "učinio",
        232: "savršen",
        235: "savršen",
        252: "savršen",
    },
    "male pilot": {
        130: "dobio",
        138: "mislio sam",
        147: "učinio",
        173: "nisam mogao",
        181: "pustio",
        183: "zaslužio",
        184: "Lagao si",
        186: "ostao sam bez noge",
        215: "pobrinuo",
        216: "Izgubio je nogu",
    },
    "male mirror man": {
        53: "Ispravio sam",
        76: "Zaglavio sam",
        93: "Riješio sam",
        99: "obećao",
    },
    "female Maddie": {
        10: "ozlijeđena",
        13: "htjela",
        14: "sigurna",
        110: "sigurna",
        151: "Nazvala sam",
        152: "zabrinula",
        157: "sigurna sam",
        194: "Htjela sam",
        201: "Nisam htjela",
        215: "Sigurna sam",
    },
    "female overseer": {
        37: "htjela",
        39: "mogla bih",
    },
}

FORBIDDEN_GENDER_REGRESSIONS = (
    "mislila sam da će ona",
    "ne bih te pustila",
    "nisam zaslužila",
    "lagala si mi",
    "ostala sam bez noge",
    "kao što sam obećala",
    "hrabra sam",
    "bit ću savršena",
)


UI_TRANSLATIONS = {
    "No": "NE",
    "Yes": "DA",
    "Nastavi": "NASTAVI",
    "Postavke": "POSTAVKE",
    "Response Button": "ODGOVOR",
    "ACT 1": "PRVI ČIN",
    "Mouse Sensitivity": "OSJETLJIVOST MIŠA",
    "OSJETLJIVOST POKAZIVAČA": "OSJETLJIVOST MIŠA",
    "(PC Name)": "(IGRAČ)",
    "(NPC Name)": "(SUGOVORNIK)",
    "Enter text...": "Upiši tekst...",
    "Label": "Oznaka",
    "(Subtitle)": "(Podnaslov)",
    "New Text": "Nova poruka",
    "Resolution": "REZOLUCIJA",
    "Are you sure you want to quit?": "Sigurno želiš izaći?",
    "MIRROR MAN": "ČOVJEK U OGLEDALU",
    "GLEDAJ UNUTRA": "ČOVJEK U OGLEDALU",
    "RATOBORAC": "RATNI HUŠKAČ",
    "MADE BY KARLOLEGEND": "AUTOR: KARLOLEGEND",
    "ODNOSI": "HLADNI GRAD",
    "Izlaz": "IZLAZ",
}


def decode_yaml_scalar(raw: str) -> str:
    raw = raw.strip()
    if not raw:
        return ""
    if raw.startswith('"'):
        return json.loads(raw)
    if raw.startswith("'"):
        return raw[1:-1].replace("''", "'")
    return raw


def dialogue_slots(lines: list[str]) -> list[tuple[int, str]]:
    slots: list[tuple[int, str]] = []
    for index, line in enumerate(lines[:-1]):
        if line.strip() != "- title: Dialogue Text":
            continue
        value_line = lines[index + 1]
        match = re.fullmatch(r"(\s*)value:(?:\s(.*))?", value_line)
        if match is None:
            raise ValueError(f"Malformed Dialogue Text value at line {index + 2}.")
        slots.append((index + 1, decode_yaml_scalar(match.group(2) or "")))
    return slots


def source_dialogue_text() -> str:
    result = subprocess.run(
        ["git", "show", f"{SOURCE_REVISION}:{DIALOGUE_PATH.as_posix()}"],
        cwd=ROOT,
        check=True,
        capture_output=True,
    )
    return result.stdout.decode("utf-8")


def rewrite_dialogue(current_text: str) -> tuple[str, int]:
    source_lines = source_dialogue_text().splitlines()
    current_lines = current_text.splitlines()
    source_slots = dialogue_slots(source_lines)
    current_slots = dialogue_slots(current_lines)

    if len(source_slots) != 538 or len(current_slots) != 538:
        raise ValueError(
            f"Expected 538 Dialogue Text fields; source={len(source_slots)}, "
            f"current={len(current_slots)}."
        )

    unique_source_values = list(
        dict.fromkeys(value for _, value in source_slots if value)
    )
    if len(unique_source_values) != 257 or len(FINAL_TRANSLATIONS) != 257:
        raise ValueError(
            f"Expected 257 translations; source={len(unique_source_values)}, "
            f"Croatian={len(FINAL_TRANSLATIONS)}."
        )

    translations = dict(zip(unique_source_values, FINAL_TRANSLATIONS, strict=True))
    changed = 0
    for (line_index, current), (_, source) in zip(
        current_slots, source_slots, strict=True
    ):
        if not source:
            continue
        translated = translations[source]
        if current != translated:
            changed += 1
        indentation = re.match(r"\s*", current_lines[line_index]).group(0)
        current_lines[line_index] = (
            f"{indentation}value: "
            + json.dumps(translated, ensure_ascii=False)
        )

    rewritten = "\n".join(current_lines) + "\n"
    if len(rewritten.splitlines()) != len(current_text.splitlines()):
        raise ValueError("Dialogue rewrite changed the serialized line count.")
    return rewritten, changed


def rewrite_scene(scene_text: str, scene_path: Path) -> tuple[str, int]:
    lines = scene_text.splitlines()
    changed = 0
    natrag_components = 0

    for index, line in enumerate(lines):
        if not line.startswith("  m_text: "):
            continue
        current = decode_yaml_scalar(line[10:])
        translated = UI_TRANSLATIONS.get(current)
        if translated is not None and translated != current:
            lines[index] = "  m_text: " + json.dumps(translated, ensure_ascii=False)
            current = translated
            changed += 1

        if current != "Natrag":
            continue
        natrag_components += 1
        for property_index in range(index + 1, min(index + 100, len(lines))):
            if lines[property_index].startswith("--- !u!"):
                break
            if lines[property_index].startswith("  m_TextWrappingMode: "):
                if lines[property_index] != "  m_TextWrappingMode: 0":
                    lines[property_index] = "  m_TextWrappingMode: 0"
                    changed += 1
                break
        else:
            raise ValueError(f"Natrag wrapping property missing in {scene_path}.")

    if natrag_components != 1:
        raise ValueError(
            f"Expected one Natrag component in {scene_path}; found {natrag_components}."
        )

    rewritten = "\n".join(lines) + "\n"
    if len(rewritten.splitlines()) != len(scene_text.splitlines()):
        raise ValueError(f"Scene rewrite changed the line count in {scene_path}.")
    return rewritten, changed


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def validate_gender_context() -> None:
    for role, expectations in GENDER_CONTEXT_GUARDS.items():
        for translation_number, required_text in expectations.items():
            translation = FINAL_TRANSLATIONS[translation_number - 1]
            if required_text.casefold() not in translation.casefold():
                raise ValueError(
                    f"Translation {translation_number:03d} no longer agrees with "
                    f"{role}: expected '{required_text}'."
                )

    combined_text = "\n".join(FINAL_TRANSLATIONS).casefold()
    for forbidden_text in FORBIDDEN_GENDER_REGRESSIONS:
        if forbidden_text.casefold() in combined_text:
            raise ValueError(
                f"Gender regression detected: '{forbidden_text}'."
            )

    male_title_cards = {
        "MIRROR MAN": "ČOVJEK U OGLEDALU",
        "SPASITELJ": "SPASITELJ",
        "RATOBORAC": "RATNI HUŠKAČ",
    }
    for source_text, expected_text in male_title_cards.items():
        translated_text = UI_TRANSLATIONS.get(source_text, source_text)
        if translated_text != expected_text:
            raise ValueError(
                f"Male title card '{source_text}' must remain '{expected_text}'."
            )


def make_backups(targets: dict[Path, str]) -> None:
    BACKUP_DIR.mkdir(exist_ok=True)
    manifest_path = BACKUP_DIR / "manifest.json"
    manifest = {}
    if manifest_path.exists():
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))

    for relative_path in targets:
        source = ROOT / relative_path
        backup_name = relative_path.as_posix().replace("/", "__")
        destination = BACKUP_DIR / backup_name
        if not destination.exists():
            shutil.copy2(source, destination)
        backup_bytes = destination.read_bytes()
        manifest[relative_path.as_posix()] = {
            "backup": backup_name,
            "sha256": sha256(backup_bytes),
            "bytes": len(backup_bytes),
        }

    manifest_path.write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
        newline="\n",
    )


def main() -> None:
    validate_gender_context()

    targets = {
        DIALOGUE_PATH: (ROOT / DIALOGUE_PATH).read_text(encoding="utf-8"),
        **{
            scene_path: (ROOT / scene_path).read_text(encoding="utf-8")
            for scene_path in SCENE_PATHS
        },
    }

    rewritten: dict[Path, str] = {}
    rewritten[DIALOGUE_PATH], dialogue_changes = rewrite_dialogue(
        targets[DIALOGUE_PATH]
    )
    scene_changes = 0
    for scene_path in SCENE_PATHS:
        rewritten[scene_path], changes = rewrite_scene(
            targets[scene_path], scene_path
        )
        scene_changes += changes

    make_backups(targets)
    for relative_path, content in rewritten.items():
        (ROOT / relative_path).write_text(
            content,
            encoding="utf-8",
            newline="\n",
        )

    print(
        f"Croatian overhaul complete: {dialogue_changes} dialogue fields and "
        f"{scene_changes} scene properties changed."
    )
    print(f"Exact pre-overhaul backups: {BACKUP_DIR}")


if __name__ == "__main__":
    main()
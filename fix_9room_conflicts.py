from pathlib import Path
import re
import shutil
import sys

path = Path("Assets/Scenes/9Room.unity")

if not path.exists():
    print("Dosya bulunamadı:", path)
    print("Bu script'i Unity proje klasörünün ana dizininde çalıştırmalısın.")
    sys.exit(1)

backup = path.with_suffix(path.suffix + ".backup_before_conflict_fix")
shutil.copy2(path, backup)

text = path.read_text(encoding="utf-8", errors="replace")
original = text

# 1) Main Camera viewport conflict
text = re.sub(
    r"""m_NormalizedViewPortRect:\n    serializedVersion: 2\n    x: 0\n<<<<<<< Updated upstream\n    y: 0\n<<<<<<< HEAD\n    width: 2\.7455196\n    height: 1\.4158164\n=======\n    y: 0\.0020242915\n    width: 0\.9995436\n    height: 0\.9959514\n>>>>>>> Stashed changes\n=======\n    width: 1\n    height: 1\n    width: 0\.9995436\n    height: 0\.9991673\n>>>>>>> main""",
    """m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1""",
    text,
    flags=re.MULTILINE,
)

# 2) Main Camera transform position conflict
text = re.sub(
    r"""m_LocalRotation: \{x: 0, y: 0, z: 0, w: 1\}\n<<<<<<< HEAD\n<<<<<<< Updated upstream\n  m_LocalPosition: \{x: -0\.67538023, y: -0\.9830998, z: -10\}\n=======\n  m_LocalPosition: \{x: -0\.6802845, y: -0\.58268636, z: -10\.006794\}\n>>>>>>> Stashed changes\n=======\n  m_LocalPosition: \{x: -2\.8517032, y: -0\.4694109, z: -10\.006794\}\n  m_LocalPosition: \{x: -0\.50233555, y: -0\.57846206, z: -10\.006794\}\n>>>>>>> main""",
    """m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: -0.50233555, y: -0.57846206, z: -10.006794}""",
    text,
    flags=re.MULTILINE,
)

# 3) Empty Cinemachine component conflict before stripped heart UI
text = re.sub(
    r"""m_Script: \{fileID: 11500000, guid: ac0b09e7857660247b1477e93731de29, type: 3\}\n  m_Name: \n  m_EditorClassIdentifier: \n<<<<<<< HEAD\n<<<<<<< Updated upstream\n=======\n=======\n>>>>>>> main\n--- !u!114 &2080938564 stripped""",
    """m_Script: {fileID: 11500000, guid: ac0b09e7857660247b1477e93731de29, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
--- !u!114 &2080938564 stripped""",
    text,
    flags=re.MULTILINE,
)

# 4) Remove conflict markers around enemy prefab section, keep the main branch enemy prefabs.
text = text.replace("<<<<<<< HEAD\n>>>>>>> Stashed changes\n=======\n--- !u!1001 &5661446089905881962", "--- !u!1001 &5661446089905881962")
text = text.replace("<<<<<<< HEAD\r\n>>>>>>> Stashed changes\r\n=======\r\n--- !u!1001 &5661446089905881962", "--- !u!1001 &5661446089905881962")

# 5) In the Skeleton2 prefab block, remove duplicate wrong source prefab line if it appears before >>>>>>> main.
text = text.replace(
    "  m_SourcePrefab: {fileID: 100100000, guid: 8139df8a59ce7574aab6f4da6d97b26f, type: 3}\n  m_SourcePrefab: {fileID: 100100000, guid: 036d1033622108a4eaa24b17af79c23a, type: 3}\n>>>>>>> main",
    "  m_SourcePrefab: {fileID: 100100000, guid: 8139df8a59ce7574aab6f4da6d97b26f, type: 3}",
)
text = text.replace(
    "  m_SourcePrefab: {fileID: 100100000, guid: 8139df8a59ce7574aab6f4da6d97b26f, type: 3}\r\n  m_SourcePrefab: {fileID: 100100000, guid: 036d1033622108a4eaa24b17af79c23a, type: 3}\r\n>>>>>>> main",
    "  m_SourcePrefab: {fileID: 100100000, guid: 8139df8a59ce7574aab6f4da6d97b26f, type: 3}",
)

# 6) SceneRoots conflict resolution.
text = re.sub(
    r"""<<<<<<< HEAD\n<<<<<<< Updated upstream\n=======\n  - \{fileID: 956225307\}\n=======\n  - \{fileID: 956225307\}\n  - \{fileID: 2058868144\}\n  - \{fileID: 2701929337103184646\}\n  - \{fileID: 997436251\}\n  - \{fileID: 5661446089905881962\}\n  - \{fileID: 8013568957633718524\}\n  - \{fileID: 6075744687240448972\}\n>>>>>>> main""",
    """  - {fileID: 956225307}
  - {fileID: 5661446089905881962}
  - {fileID: 8013568957633718524}
  - {fileID: 6075744687240448972}""",
    text,
    flags=re.MULTILINE,
)

# Safety check: do not save if any conflict markers remain.
remaining = [marker for marker in ("<<<<<<<", "=======", ">>>>>>>") if marker in text]
if remaining:
    print("Hâlâ conflict marker kaldı:", ", ".join(remaining))
    print("Dosya değiştirilmedi. Backup oluşturuldu:", backup)
    print("Kalan markerları VS Code içinde aratıp kontrol et.")
    sys.exit(2)

path.write_text(text, encoding="utf-8", newline="\n")

print("Tamamlandı.")
print("Düzeltilen dosya:", path)
print("Backup:", backup)
print("Unity'yi tekrar açmadan önce dosyada <<<<<<<, =======, >>>>>>> araması yapabilirsin.")

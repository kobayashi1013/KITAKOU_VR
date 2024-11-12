import os
import re

def get_class_info(file_path):
    with open(file_path, "r", encoding="utf-8-sig") as file:
        content = file.read()

    class_pattern = re.compile(r"class\s+(\w+)")
    member_pattern = re.compile(r"(public|private|protected)\s+(\w+)\s(\w+);")

    classes = []
    for class_match in class_pattern.finditer(content):
        class_name = class_match.group(1)
        
        members = []
        for member_match in member_pattern.finditer(content, class_match.end()):
            member_type = member_match.group(2)
            member_name = member_match.group(3)
            members.append((member_type, member_name))
        
        classes.append((class_name, members))

    return classes

def generate_plantuml_content(classes):
    lines = ["@startuml"]

    for class_name, members in classes:
        lines.append(f"class {class_name} {{")
        for member_type, member_name in members:
            lines.append(f"  {member_type} {member_name}")
        lines.append("}")

    lines.append("@enduml")
    return "\n".join(lines)

scripts_dir = "Assets/Scripts"
output_file = "ClassDiagram.puml"

class_infos = []
for root, dirs, files in os.walk(scripts_dir):
    for file in files:
        if file.endswith(".cs"):
            file_path = os.path.join(root, file)
            classes = get_class_info(file_path)
            class_infos.extend(classes)

plantuml_content = generate_plantuml_content(class_infos)

with open(output_file, "w") as file:
    file.write(plantuml_content)

print("Class Diagram generated as ClassDiagram.puml")
            
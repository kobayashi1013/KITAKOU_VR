import os
import re

def get_namespace(file_path):
    with open(file_path, "r", encoding="shift_jis") as file:
        content = file.read()

    namespace_pattern = re.compile(r"namespace\s+([\w\.]+)")
    class_pattern = re.compile(r"class\s+(\w+)")
    member_pattern = re.compile(r"(public)\s+(\w+)\s+(\w+);")

    namespaces = []
    for namespace_match in namespace_pattern.finditer(content):
        namespace_name = namespace_match.group(1)
        namespace_start = namespace_match.end()
        namespace_end = len(content)

        next_namespace = namespace_pattern.search(content, namespace_start)
        if next_namespace:
            namespace_end = next_namespace.start()

        namespace_content = content[namespace_start:namespace_end]
        classes = []
        for class_match in class_pattern.finditer(namespace_content):
            class_name = class_match.group(1)
            class_start = class_match.end()
            class_end = namespace_end

            next_class = class_pattern.search(namespace_content, class_start)
            if next_class:
                class_end = next_class.start()

            class_content = namespace_content[class_start:class_end]
            members = []
            for member_match in member_pattern.finditer(class_content):
                member_access = member_match.group(1)
                member_type = member_match.group(2)
                member_name = member_match.group(3)
                members.append((member_access, member_type, member_name))
            
            classes.append((class_name, members))

        namespaces.append((namespace_name, classes))

    return namespaces                

def generate_plantuml_content(namespaces):
    lines = ["@startuml"]

    for namespace_name, classes in namespaces:
        lines.append(f"namespace {namespace_name} {{")
        for class_name, members in classes:
            lines.append(f"  class {class_name} {{")
            for member_access, member_type, member_name in members:
                lines.append(f"    {member_access} {member_type} {member_name}")
            lines.append("  }")
        lines.append("}")

    lines.append("@enduml")
    return "\n".join(lines)

def main():
    scripts_directory_path = "Assets/Scripts"
    output_file_path = "uml.puml"

    namespaces = []
    for root, dirs, files in os.walk(scripts_directory_path):
        for file in files:
            if file.endswith(".cs"):
                file_path = os.path.join(root, file)
                namespace = get_namespace(file_path)
                namespaces.extend(namespace)

    plantuml_content = generate_plantuml_content(namespaces)

    with open(output_file_path, "w") as file:
        file.write(plantuml_content)

    print("UML source generated as uml.puml")

if __name__ == "__main__":
    main()
            
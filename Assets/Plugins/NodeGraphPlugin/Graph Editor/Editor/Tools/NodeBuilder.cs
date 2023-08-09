
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugin.NodeGraph.Editor {
    public class NodeBuilder {

        //To reproduce the permormance issues with property fields outcommand line 23 and command line 22
        public static void DrawAndBindUI(string PropertyPathUIList, string PropertyPathNodeUIGuid, NodeGraphContainerSO dataContainerSO, BaseNode node) {
            //SerializedObject serializedObject = new SerializedObject(dataContainerSO); 
            var serializedObject = Helper.nodeGraphContainer;
            var ui = serializedObject.FindProperty(PropertyPathUIList);

            for (int x = 0; x < ui.arraySize; x++) {
                var property = ui.GetArrayElementAtIndex(x);
                if (property.FindPropertyRelative(PropertyPathNodeUIGuid).stringValue.Equals(node.NodeGuid)) {
                    BuildGUIWithPropertyField(property, node.mainContainer);
                    //BuildGUIWithoutPropertyFields(property, node.mainContainer);
                }
            }
            // VERY IMPORTANT, WE GET AUTO BINDING WITH THIS (CHANGING VALUES WILL BE DIRECTLY REFLECTED IN THE SCRIPTABLE OBJECT)
            node.Bind(ui.serializedObject);

        }

        //Solution with property fields using IMGUI is very imperformant
        public static void BuildGUIWithPropertyField(SerializedProperty property, VisualElement container) {
            // get all members of this property
            List<MemberInfo> members = new List<MemberInfo>(GetTypeOfSerializedProperty(property).GetMembers());

            // get the last property
            SerializedProperty endProperty = property.GetEndProperty();

            // check if we have visible properties
            if (!property.NextVisible(true)) return;
            do {
                // check if the current property is equal to the last properity
                if (SerializedProperty.EqualContents(property, endProperty)) {
                    break;
                }

                //create the propertyfield
                PropertyField propertyField = new PropertyField(property.Copy()) {
                    name = property.name,
                    bindingPath = property.propertyPath,
                };

                //handle specific attributes
                HandleAttributesForProperty(members, property, container);

                // hide m_Script field
                if (property.propertyPath == "m_Script") {
                    propertyField.SetEnabled(value: false);
                }
                // add the new propertyField to the given container
                container.Add(propertyField);
            } while (property.NextVisible(false)); // go to the next property element
        }

        //Individual builder with switch case solution (just to show up performance boost)
        public static void BuildGUIWithoutPropertyFields(SerializedProperty property, VisualElement container) {

            List<MemberInfo> members = new List<MemberInfo>(GetTypeOfSerializedProperty(property).GetMembers());
            SerializedProperty endProperty = property.GetEndProperty();
            List<SerializedProperty> propertiers = new List<SerializedProperty>();

            if (!property.NextVisible(true)) return;

            do {

                if (SerializedProperty.EqualContents(property, endProperty)) {
                    break;
                }

                Debug.Log(GetTypeOfSerializedProperty(property).Name);

                var field = new VisualElement();

                switch (GetTypeOfSerializedProperty(property).Name) {

                    case "String":
                        TextField fi1 = new TextField(property.name);
                        fi1.bindingPath = property.propertyPath;
                        field = fi1;
                        field = field as TextField;
                        break;
                    case "Vector2":
                        Vector2Field fi2 = new Vector2Field(property.name);
                        fi2.bindingPath = property.propertyPath;
                        field = fi2;
                        field = field as Vector2Field;
                        break;
                    case "Int32":
                        IntegerField fi3 = new IntegerField(property.name);
                        fi3.bindingPath = property.propertyPath;
                        field = fi3;
                        field = field as IntegerField;
                        break;
                    case "DefaultPortTempalte":
                        BuildGUIWithoutPropertyFields(property, container);
                        break;
                    case "HappinessEffectRewardNodeData":
                        BuildGUIWithoutPropertyFields(property, container);
                        break;
                    case "EndNodeData":
                        BuildGUIWithoutPropertyFields(property, container);
                        break;
                    case "StartNodeData":
                        BuildGUIWithoutPropertyFields(property, container);
                        break;
                    default:
                        break;
                }

                // hide m_Script field
                if (property.propertyPath == "m_Script") {
                    field.SetEnabled(value: false);
                }

                HandleAttributesForProperty(members, property, container);
                container.Add(field);

            } while (property.NextVisible(false)); //--> InvalidOperationException: The operation
                                                   //is not possible when moved past all properties
                                                   //(Next returned false) 

        }

        public static Type GetTypeOfSerializedProperty(SerializedProperty property) {
            // This is based on a sophisticated solution found here
            // Source: https://answers.unity.com/questions/1347203/a-smarter-way-to-get-the-type-of-serializedpropert.html?page=2&pageSize=5&sort=votes
            FieldInfo _GetFieldViaPath(Type type, string propertyPath) {

                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var parent = type;
                var fi = parent.GetField(propertyPath, flags);
                var paths = propertyPath.Split('.');

                for (int i = 0; i < paths.Length; i++) {
                    fi = parent.GetField(paths[i], flags);
                    if (fi != null) {
                        // there are only two container field type that can be serialized:
                        // Array and List<T>
                        if (fi.FieldType.IsArray) {
                            parent = fi.FieldType.GetElementType();
                            i += 2;
                            continue;
                        }

                        if (fi.FieldType.IsGenericType) {
                            parent = fi.FieldType.GetGenericArguments()[0];
                            i += 2;
                            continue;
                        }
                        parent = fi.FieldType;
                    } else {
                        break;
                    }

                }
                if (fi == null) {
                    if (type.BaseType != null) {
                        return _GetFieldViaPath(type.BaseType, propertyPath);
                    } else {
                        return null;
                    }
                }

                return fi;
            }

            // here we'll use our inline method to actually get the field from our SerializedProperty
            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fieldInfo = _GetFieldViaPath(parentType, property.propertyPath);

            if (fieldInfo != null) {
                return fieldInfo.FieldType;
            } else {
                return null;
            }
        }
        private static void HandleAttributesForProperty(List<MemberInfo> members, SerializedProperty property, VisualElement container) {
            // We currently only support Header & Space attributes
            // Please extend this list if we want to support more
            MemberInfo member = members.Find(x => x.Name == property.name);
            if (member != null) {
                IEnumerable<Attribute> headers = member.GetCustomAttributes(typeof(HeaderAttribute));
                IEnumerable<Attribute> spaces = member.GetCustomAttributes(typeof(SpaceAttribute));

                foreach (Attribute x in headers) {
                    HeaderAttribute actual = (HeaderAttribute)x;
                    Label header = new Label { text = actual.header };
                    header.style.unityFontStyleAndWeight = FontStyle.Bold;
                    container.Add(new Label { text = " ", name = "Header Spacer" });
                    container.Add(header);
                }
                foreach (Attribute unused in spaces) {
                    container.Add(new Label { text = " " });
                }
            }
        }
    }
}

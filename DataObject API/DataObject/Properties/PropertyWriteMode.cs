namespace FF7R2.DataObject.Properties;

internal enum PropertyWriteMode {
    MAIN_OBJ_ONLY, // The main entry properties, headers, etc, with placeholders written for the proxy objects.
    SUB_OBJECTS_ONLY // All the arrays and things that need writing at an offset.
}
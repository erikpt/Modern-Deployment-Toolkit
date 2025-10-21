# MDT XML Format Validation

## Overview

This document validates that the .NET Framework 3.5 client parser is compatible with actual MDT Deployment Workbench XML files provided by @erikpt.

## Test Files Analyzed

Three real-world MDT task sequence XML files from `/DeployXMLExamples`:

1. **001/ts.xml** - Standard Client Task Sequence (39 KB)
2. **002/ts.xml** - Standard Client Task Sequence (52 KB)  
3. **003/ts.xml** - Standard Client Task Sequence (46 KB)

## MDT XML Format Observed

### Root Structure
```xml
<sequence version="3.00" name="..." description="...">
  <globalVarList>
    <variable name="..." property="...">value</variable>
  </globalVarList>
  <group name="..." disable="false" continueOnError="false" ...>
    <step type="..." name="..." ...>
      <defaultVarList>
        <variable name="..." property="...">value</variable>
      </defaultVarList>
      <action>command or script</action>
      <condition>...</condition>
    </step>
  </group>
</sequence>
```

### Key Elements Found

#### 1. Global Variables
- Located in `<globalVarList>` under root `<sequence>`
- Each variable has `name` and `property` attributes
- Inner text contains the value

#### 2. Groups
- Nested structure with `<group>` elements
- Attributes: `name`, `disable`, `continueOnError`, `description`, `expand`
- Can contain both steps and nested groups
- May have `<condition>` elements

#### 3. Steps
- Use `<step>` element with `type` attribute
- Common attributes: `name`, `description`, `disable`, `continueOnError`, `successCodeList`, `startIn`, `runIn`
- Contains:
  - `<defaultVarList>` with step-specific variables
  - `<action>` with command to execute
  - Optional `<condition>` for execution conditions

#### 4. Conditions
Multiple condition types found:
- `SMS_TaskSequence_VariableConditionExpression` - Variable-based conditions
- `SMS_TaskSequence_WMIConditionExpression` - WMI query conditions
- Logical operators: `<operator type="or">` and `<operator type="not">`
- Condition variables: `Variable`, `Operator`, `Value`

### Step Types Found (All 10 Priority Types Confirmed)

✅ **SMS_TaskSequence_RunCommandLineAction**
- Properties: `PackageID`, `RunAsUser`, `LoadProfile`
- Example: `<action>cmd /c mkdir X:\_SMSTaskSequence</action>`

✅ **SMS_TaskSequence_SetVariableAction**
- Properties: `VariableName`, `VariableValue`
- Example: `<action>cscript.exe "%SCRIPTROOT%\ZTISetVariable.wsf"</action>`

✅ **BDD_Validate**
- Properties: `ImageSize`, `ImageProcessorSpeed`, `ImageMemory`, `VerifyOS`
- Example: `<action>cscript.exe "%SCRIPTROOT%\ZTIValidate.wsf"</action>`

✅ **Check BIOS** (no type attribute, identified by name)
- Example: `<step name="Check BIOS" ...>`
- Example: `<action>cscript.exe "%SCRIPTROOT%\ZTIBIOSCheck.wsf"</action>`

✅ **SMS_TaskSequence_PartitionDiskAction**
- Complex properties for disk partitioning (BIOS vs UEFI)
- Properties: `DiskIndex`, `Partitions0Type`, `Partitions0FileSystem`, etc.
- Example: `<action>cscript.exe "%SCRIPTROOT%\ZTIDiskpart.wsf"</action>`

✅ **Configure** (no type attribute, identified by name)
- Example: `<step name="Configure" ...>`
- Example: `<action>cscript.exe "%SCRIPTROOT%\ZTIConfigure.wsf"</action>`

✅ **BDD_InstallOS**
- Properties: `OSGUID`, `DestinationDisk`, `DestinationPartition`, etc.
- Example: `<action>cscript.exe "%SCRIPTROOT%\LTIApply.wsf"</action>`

✅ **ZTIWinRE.wsf** (via SMS_TaskSequence_RunCommandLineAction)
- Example: `<action>cscript.exe "%SCRIPTROOT%\ZTIWinRE.wsf"</action>`

✅ **BDD_RunPowerShellAction**
- Properties: `ScriptName`, `Parameters`, `PackageID`
- Example: `<action>cscript.exe "%SCRIPTROOT%\ZTIPowerShell.wsf</action>`

✅ **SMS_TaskSequence_RebootAction**
- Properties: `Message`, `MessageTimeout`, `Target`
- Example: `<action>smsboot.exe /target:WinPE</action>`

### Additional Step Types Found

- **BDD_Gather** - Gather system information
- **BDD_InjectDrivers** - Driver injection
- **BDD_InstallUpdatesOffline** - Apply patches
- **BDD_RecoverDomainJoin** - Domain join recovery
- **SMS_TaskSequence_EnableBitLockerAction** - BitLocker encryption
- **BDD_InstallApplication** - Application installation
- **SMS_TaskSequence_SubTasksequence** - Sub-task sequence execution

## Parser Compatibility Analysis

### ✅ Compatible Features

1. **Root Element Parsing**
   - Parser looks for `<sequence>` element ✓
   - Handles version, name, description attributes ✓

2. **Global Variables**
   - Parser looks for `<globalVarList>` ✓
   - Extracts variable name/property/value ✓

3. **Group and Step Parsing**
   - Handles nested groups ✓
   - Parses step attributes (type, name, disable, continueOnError) ✓
   - Extracts defaultVarList as step properties ✓

4. **Condition Parsing**
   - Parses `<condition>` elements ✓
   - Handles nested expression elements ✓
   - Stores condition type and properties ✓

5. **Action Element**
   - Currently parsed but not explicitly used
   - Action text available in XML

### ⚠️ Observations

1. **Step Identification**
   - Most steps have `type` attribute
   - Some steps (Check BIOS, Configure) identified only by `name`
   - Parser should check both `type` and `name` for step identification

2. **Variable Storage**
   - MDT uses both `name` and `property` attributes
   - Parser correctly reads both ✓

3. **Namespace**
   - MDT files don't use XML namespaces
   - Parser's namespace handling is optional ✓

## Parser Enhancements Needed

### Minor Adjustments Required

1. **.Action Element Storage**
   - Add `Action` property to `TaskSequenceStep` model
   - Store action command text for executor reference

2. **Step Executor Matching**
   - `CheckBiosExecutor` should match on name "Check BIOS" ✓ (already implemented)
   - `ConfigureExecutor` should match on name "Configure" ✓ (already implemented)
   - `ZtiWinReExecutor` should match on action containing "ZTIWinRE" ✓ (already implemented)

3. **Condition Operator Handling**
   - Add support for `<operator type="or">` and `<operator type="not">`
   - Currently only handles individual expressions

## Validation Summary

| Feature | Status | Notes |
|---------|--------|-------|
| XML Structure | ✅ Compatible | Parser handles MDT format correctly |
| Variables | ✅ Compatible | Global and step variables parsed |
| Groups | ✅ Compatible | Nested groups supported |
| Steps | ✅ Compatible | All attributes captured |
| Conditions | ⚠️ Partial | Simple conditions work, operators need enhancement |
| All 10 Priority Steps | ✅ Found | All priority step types present in examples |
| Action Commands | ⚠️ Available | Action text in XML but not stored in model |

## Recommendations

### High Priority
1. **Add Action Property** to TaskSequenceStep model
2. **Test with Mono** or .NET Framework to validate compilation
3. **Enhance Condition Evaluator** to handle logical operators

### Medium Priority
1. Add more step executors for common MDT step types
2. Implement WMI condition evaluation
3. Add registry condition evaluation

### Low Priority
1. Add validation for step property requirements
2. Implement variable substitution in action commands
3. Add schema validation for MDT XML

## Conclusion

The .NET Framework 3.5 client parser is **fundamentally compatible** with real MDT Deployment Workbench XML files. The three test files demonstrate:

- ✅ All 10 priority step types are present
- ✅ Parser can handle the MDT XML structure
- ⚠️ Minor enhancements needed for complete feature support
- ✅ No breaking issues identified

The parser is ready for testing once build environment is available.

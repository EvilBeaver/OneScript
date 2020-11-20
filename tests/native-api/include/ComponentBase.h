/*
 *          Warning!!!
 *       DO NOT ALTER THIS FILE!
 */


#ifndef __COMPONENT_BASE_H__
#define __COMPONENT_BASE_H__

#include "types.h"
////////////////////////////////////////////////////////////////////////////////
/**
 *  The given interface is intended for initialization and 
 *  uninitialization of component and its adjustments
 */
/// Interface of component initialization.
class IInitDoneBase
{
public:
    virtual ~IInitDoneBase() {}
    /// Initializes component
    /**
     *  @param disp - 1C:Enterpise interface
     *  @return the result of
     */
    virtual bool ADDIN_API Init(void* disp) = 0;
    /// Sets the memory manager
    /*
     * @param mem - pointer to memory manager interface.
     *  @return the result of
     */
    virtual bool ADDIN_API setMemManager(void* mem) = 0;

    /// Returns component version
    /**
     *  @return - component version (2000 - version 2)
     */
    virtual long ADDIN_API GetInfo() = 0;

    /// Uninitializes component
    /**
     *  Component here should release all consumed resources.
     */
    virtual void ADDIN_API Done() = 0;

};
///////////////////////////////////////////////////////////////////////////
/**
 *  The given interface defines methods that are intented to be used by the Platform
 */
/// Interface describing extension of language.
class ILanguageExtenderBase
{
public:
    virtual ~ILanguageExtenderBase(){}
    /// Registers language extension
    /**
     *  @param wsExtensionName - extension name
     *  @return the result of
     */
    virtual bool ADDIN_API RegisterExtensionAs(WCHAR_T** wsExtensionName) = 0;

    /// Returns number of component properties
    /**
     *  @return number of properties
     */
    virtual long ADDIN_API GetNProps() = 0;

    /// Finds property by name
    /**
     *  @param wsPropName - property name
     *  @return property index or -1, if it is not found
     */
    virtual long ADDIN_API FindProp(const WCHAR_T* wsPropName) = 0;

    /// Returns property name
    /**
     *  @param lPropNum - property index (starting with 0)
     *  @param lPropAlias - 0 - international alias,
     *      1 - russian alias. (International alias is required)
     *  @return proeprty name or 0 if it is not found
     */
    virtual const WCHAR_T* ADDIN_API GetPropName(long lPropNum,  
						 long lPropAlias) = 0;

    /// Returns property value
    /**
     *  @param lPropNum - property index (starting with 0)
     *  @param pvarPropVal - the pointer to a variable for property value
     *  @return the result of
     */
    virtual bool ADDIN_API GetPropVal(const long lPropNum, 
				      tVariant* pvarPropVal) = 0;

    /// Sets the property value
    /**
     *  @param lPropNum - property index (starting with 0)
     *  @param varPropVal - the pointer to a variable for property value
     *  @return the result of
     */
    virtual bool ADDIN_API SetPropVal(const long lPropNum, 
				      tVariant* varPropVal) = 0;

    /// Is property readable?
    /**
     *  @param lPropNum - property index (starting with 0)
     *  @return true if property is readable
     */
    virtual bool ADDIN_API IsPropReadable(const long lPropNum) = 0;

    /// Is property writable?
    /**
     *  @param lPropNum - property index (starting with 0)
     *  @return true if property is writable
     */
    virtual bool ADDIN_API IsPropWritable(const long lPropNum) = 0;

    /// Returns number of component methods
    /**
     *  @return number of component  methods
     */
    virtual long ADDIN_API GetNMethods() = 0;

    /// Finds a method by name 
    /**
     *  @param wsMethodName - method name
     *  @return - method index
     */
    virtual long ADDIN_API FindMethod(const WCHAR_T* wsMethodName) = 0;

    /// Returns method name
    /**
     *  @param lMethodNum - method index(starting with 0)
     *  @param lMethodAlias - 0 - international alias,
     *      1 - russian alias. (International alias is required)
     *  @return method name or 0 if method is not found
     */
    virtual const WCHAR_T* ADDIN_API GetMethodName(const long lMethodNum, 
						   const long lMethodAlias) = 0;

    /// Returns number of method parameters
    /**
     *  @param lMethodNum - method index (starting with 0)
     *  @return number of parameters
     */
    virtual long ADDIN_API GetNParams(const long lMethodNum) = 0;

    /// Returns default value of method parameter
    /**
     *  @param lMethodNum - method index(starting with 0)
     *  @param lParamNum - parameter index (starting with 0)
     *  @param pvarParamDefValue - the pointer to a variable for default value
     *  @return the result of
     */
    virtual bool ADDIN_API GetParamDefValue(const long lMethodNum, 
					    const long lParamNum,
					    tVariant *pvarParamDefValue) = 0;

    /// Does the method have a return value?
    /**
     *  @param lMethodNum - method index (starting with 0)
     *  @return true if the method has a return value
     */
    virtual bool ADDIN_API HasRetVal(const long lMethodNum) = 0;

    /// Calls the method as a procedure
    /**
     *  @param lMethodNum - method index (starting with 0)
     *  @param paParams - the pointer to array of method parameters
     *  @param lSizeArray - the size of array
     *  @return the result of
     */
    virtual bool ADDIN_API CallAsProc(const long lMethodNum,
				      tVariant* paParams, 
				      const long lSizeArray) = 0;

    /// Calls the method as a function
    /**
     *  @param lMethodNum - method index (starting with 0)
     *  @param pvarRetValue - the pointer to returned value
     *  @param paParams - the pointer to array of method parameters
     *  @param lSizeArray - the size of array
     *  @return the result of
     */
    virtual bool ADDIN_API CallAsFunc(const long lMethodNum,
				      tVariant* pvarRetValue, 
				      tVariant* paParams, 
				      const long lSizeArray) = 0;    
};
///////////////////////////////////////////////////////////////////////////
/**
 *  This interface is used to change component locale
 */
/// Base interface for component localization.
class LocaleBase
{
public:
    virtual ~LocaleBase(){}
    /// Changes component locale
    /**
     *  @param loc - new locale (for Windows - rus_RUS, 
     *      for Linux - ru_RU, etc...)
     */
    virtual void ADDIN_API SetLocale(const WCHAR_T* loc) = 0;
};

///////////////////////////////////////////////////////////////////////////
/**
 *  The given interface is generalized, for its obligatory inheritance 
 *  in implementing components.
 */
///  Base interface describing object as a set of properties and methods.
class IComponentBase :
public IInitDoneBase,
    public ILanguageExtenderBase,
    public LocaleBase
{
public:
    virtual ~IComponentBase(){}
};

enum AppCapabilities
{
    eAppCapabilitiesInvalid = -1,
    eAppCapabilities1 = 1,
    eAppCapabilitiesLast = eAppCapabilities1,
};

/// Announcements of exported functions
/**
 * These functions should be implemented that component can be loaded and created. 
 */
extern "C" long GetClassObject(const WCHAR_T*, IComponentBase** pIntf);
extern "C" long DestroyObject(IComponentBase** pIntf);
extern "C" const WCHAR_T* GetClassNames();
extern "C" AppCapabilities SetPlatformCapabilities(const AppCapabilities capabilities);

typedef long (*GetClassObjectPtr)(const WCHAR_T* wsName, IComponentBase** pIntf);
typedef long (*DestroyObjectPtr)(IComponentBase** pIntf);
typedef const WCHAR_T* (*GetClassNamesPtr)();
typedef AppCapabilities (*SetPlatformCapabilitiesPtr)(const AppCapabilities capabilities);

#endif //__COMPONENT_BASE_H__

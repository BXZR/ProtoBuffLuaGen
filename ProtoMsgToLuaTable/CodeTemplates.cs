using System;
using System.Collections.Generic;

public class CodeTemplates
{
    /**
     * table.Set<string, valueType>("key", (cast)value); //annotions
     */
    public static readonly string TPL_LINE_SET_TABLE_KVP_WITH_CAST_AND_ANNOTIONS =
@"            table.Set<{0}, {1}>('{2}', {3}msg.{2});{4}";

    /**
     * table.Set<string, LuaTable>("key", msg.itemName == null ? null : msg.itemName.ToLuaTable(luaEnv));
     */
    public static readonly string TPL_LINE_SET_TABLE_VALUE_IS_LUA_TABLE =
@"            table.Set<string, LuaTable>('{0}', msg.{0} == null ? null : msg.{0}.{1}(luaEnv));{2}";


    public static readonly string TPL_LINE_SET_TABLE_VALUE_IS_LUA_TABLES =
@"            if (msg.{0} != null && msg.{0}.Count > 0)[[
                List<LuaTable> itemTables = new List<LuaTable>();
                foreach (var item in msg.{0})[[
                    if (item == null)[[
                        continue;
                    ]]
                    itemTables.Add(item.ToLuaTable(luaEnv));
                ]]
                table.Set<string, List<LuaTable>>('{0}', itemTables);
            ]] else [[
                table.Set<string, List<LuaTable>>('{0}', null);
            ]]";

    /**
     *  LuaAPI.lua_pushstring(L, "str");
     *  LuaAPI.lua_pushstring(L, "value");
     *  LuaAPI.xlua_psettable(L, -3);
     */
    public static readonly string TPL_LINE_PUSH_KVP_VALUE_IS_CLASS = 
@"
            if ({0} == null)
            [[
                {2}.lua_pushnil(L);
            ]]
            else
            [[
                {0}.{1}(L);
            ]]
";

    public static readonly string TPL_LINE_PUSH_KVP_VALUE_IS_CLASS_2 =
@"
                    if ({0} == null)
                    [[
                        {2}.lua_pushnil(L);
                    ]]
                    else
                    [[
                        {0}.{1}(L);
                    ]]
";

    /**
     *   if (msg.itemName != null && msg.itemName.Count > 0)
     *   {
     *       var index = 0;
     *       LuaAPI.lua_newtable(L);
     *       foreach (var item in msg.itemName)
     *       {
     *           LuaAPI.xlua_pushinteger(L, index);
     *           
     *           index ++;
     *       }
     *   }
     *   else
     *   {
     *        LuaAPI.lua_pushnil(L);
     *   }
     */

    public static readonly string TPL_LINE_PUSH_KVP_VALUE_IS_GENERIC_LIST =
@"
            if ({0} != null && {0}.Count > 0)
            [[
                var index = 0;
                {1}.lua_newtable(L);
                foreach (var item in {0})
                [[
                    {1}.xlua_pushinteger(L, index);
                    {2}
                    {1}.xlua_psettable(L, -3);
                    index ++;
                ]]
            ]]
            else
            [[
                {1}.lua_pushnil(L);
            ]]
";



    /**
     *  LuaAPI.lua_pushstring(L, "str");
     *  LuaAPI.lua_pushstring(L, "value");
     *  LuaAPI.xlua_psettable(L, -3);
     */
    public static readonly string TPL_LINE_POP_LUATABLE_1 =
@"
            [[
                var subTable = table.Get<LuaTable>('{0}');
                if (subTable == null)[[
                    {1} = null;
                ]] else [[
                    {1} = new {2}().{3}(subTable);
                ]]
            ]]
";

    public static readonly string TPL_LINE_POP_LUATABLE_2 =
@"
                    [[
                        var subTable = table.Get<LuaTable>('{0}');
                        if (subTable == null)[[
                            {1} = null;
                        ]] else [[
                            {1} = new {2}().{3}(subTable);
                        ]]
                    ]]
";

    public static readonly string TPL_LINE_POP_LUATABLE_GENERIC_LIST =
@"
            [[
                var subTable = table.Get<LuaTable>('{0}');
                if (subTable == null) [[
                    {1} = null;
                ]] else [[
                    var list = new List<{2}>(subTable.Length);
                    var inst = new {2}();
                    subTable.ForEach<int, LuaTable>((index, tbl) => [[
                        list.Add(inst.{4}(tbl));
                    ]]);
                ]]
            ]]
";

    public static readonly string TPL_LINE_POP_LUATABLE_GENERIC_LIST_2 =
@"
                    [[
                        var subTable = table.Get<LuaTable>('{0}');
                        if (subTable == null) [[
                            {1} = null;
                        ]] else [[
                            var list = new List<{2}>(subTable.Length);
                            var inst = new {2}();
                            subTable.ForEach<int, LuaTable>((index, tbl) => [[
                                list.Add(inst.{4}(tbl));
                            ]]);
                        ]]
                    ]]
";


    #region 数组模板
    public static readonly string TPL_LINE_ARRAY =
@"
            if ({0} != null && {0}.Length > 0)
            [[
                var index = 0;
                {1}.lua_newtable(L);
                foreach (var item in {0})
                [[
                    {1}.xlua_pushinteger(L, index);
                    {1}{2}(L, item);
                    {1}.xlua_psettable(L, -3);
                    index ++;
                ]]
            ]]
            else
            [[
                {1}.lua_pushnil(L);
            ]]
";

    public static readonly string TPL_LINE_ARRAY_2 =
@"
                    if ({0} != null && {0}.Length > 0)
                    [[
                        var index = 0;
                        {1}.lua_newtable(L);
                        foreach (var item in {0})
                        [[
                            {1}.xlua_pushinteger(L, index);
                            {1}{2}(L, item);
                            {1}.xlua_psettable(L, -3);
                            index ++;
                        ]]
                    ]]
                    else
                    [[
                        {1}.lua_pushnil(L);
                    ]]
";
    #endregion
}

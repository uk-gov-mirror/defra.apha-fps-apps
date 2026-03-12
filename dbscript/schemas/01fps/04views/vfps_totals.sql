-- View: fps.vfps_totals

CREATE OR REPLACE VIEW fps.vfps_totals AS
 SELECT fpsyeartotals.parentproject,
    fpsyeartotals.program,
    fpsyeartotals.totaladditionalcosts,
    fpsyeartotals.totalanimalcosts,
    fpsyeartotals.totalstaffcosts,
    fpsyeartotals.totaltestcosts,
    fpsyeartotals.totalcosts,
    fpsyeartotals.custincome,
    fpsyeartotals.transferincome,
    fpsyeartotals.totalincome,
    fpsyeartotals.budget_cvl,
    fpsyeartotals.requiredprofit,
    fpsyeartotals.manager,
    fpsyeartotals.customer,
    fpsyeartotals.projectstatus,
    fpsyeartotals.pvsincome,
    fpsyeartotals.plancaseworkdebit,
    ma_a.bfbudget AS ma_bfbudget,
    fpsyeartotals.fpsyear
   FROM (fps.fpsyeartotals
     LEFT JOIN ( SELECT my_tlkpprojectradtrackdata.project,
            my_tlkpprojectradtrackdata.bfbudget
           FROM mabarchive.my_tlkpprojectradtrackdata
          WHERE ((my_tlkpprojectradtrackdata.year)::text = ( SELECT "right"((tbldb_variables.db_var_value)::text, 4) AS "right"
                   FROM fps.tbldb_variables
                  WHERE ((tbldb_variables.db_var_name)::text = 'DB_Name'::text)))) ma_a ON (((fpsyeartotals.parentproject)::text = (ma_a.project)::text)));

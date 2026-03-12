-- View: fps.vpactproject

CREATE OR REPLACE VIEW fps.vpactproject AS
 SELECT parentproject,
    projecttitle,
    program,
    customer,
    transferincome,
    budget_cvl,
    pvsincome,
    custincome AS budget_ext,
    feccost AS forecastcost,
    wip_eoy,
    wip_limit,
    wip_current,
    manager,
    projectstatus,
    projectparent,
    contract,
    disease,
    finished,
    comments,
    isdefraproject,
    costcentre,
    oracleprojectcode,
    subaccountcode,
    projectgroup,
    fpsyear
   FROM fps.tlkpproject;

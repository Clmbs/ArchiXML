CREATE DEFINER=`archi`@`%` PROCEDURE `archi1`.`update_database`()
begin
	
	
	
	/* ELEMENTS*/
		/* new objects*/
		INSERT INTO elements (model_id, id, name, documentation, type, valid_from, md5) SELECT s.model_id, s.object_id, s.name, s.documentation, s.object_type, s.date,s.md5
		FROM staging s WHERE s.target_table = 'elements' AND NOT EXISTS (SELECT 1 FROM elements e WHERE e.id = s.object_id);
		
       /* changed objects: close existing version (set valid_to) and insert new version */ 	

		INSERT INTO ELEMENTS (model_id, id, name, documentation, type, valid_from,md5) SELECT s.model_id, s.object_id, s.name, s.documentation, s.object_type, s.date,s.md5
		 FROM Staging s JOIN ELEMENTS e ON s.object_id = e.id WHERE e.md5 != s.md5 and e.valid_to is null;
		UPDATE ELEMENTS e JOIN Staging s ON e.id = s.object_id and e.valid_to is null SET e.valid_to = s.date WHERE e.md5 != s.md5;
		
		/* removed objects: close existing version (set valid_to) */
		UPDATE elements e JOIN (SELECT MAX(s.date) AS max_date FROM staging s) AS date_max
		SET e.valid_to = date_max.max_date
		WHERE NOT EXISTS (SELECT 1 FROM staging s WHERE s.object_id = e.id AND s.target_table = 'elements') 
		AND e.valid_to IS NULL;

	
	/* RELATIONS */
		/* new objects*/
		INSERT INTO relations(model_id, id, name, documentation, type, subtype, source_id,target_id, valid_from, md5) select s.model_id, s.object_id, s.name, s.documentation, s.object_type, s.subtype, s.source_id, s.target_id, s.date, s.md5
        from staging s where s.target_table = 'relations' AND NOT EXISTS (SELECT 1 FROM relations r WHERE r.id = s.object_id);

       /* changed objects: close existing version (set valid_to) and insert new version */ 

       INSERT INTO relations (model_id, id, name, documentation, `type`, subtype, source_id,target_id, valid_from,md5) 
       SELECT s.model_id, s.object_id, s.name, s.documentation, s.object_type,s.subtype, s.source_id,s.target_id, s.date,s.md5
        FROM Staging s JOIN relations r ON s.object_id = r.id WHERE r.md5 != s.md5 and r.valid_to is null ;
       UPDATE relations r JOIN Staging s ON r.id = s.object_id and r.valid_to is null SET r.valid_to = s.date WHERE r.md5 != s.md5;
       
 		/* removed objects: close existing version (set valid_to)  */
		UPDATE relations r JOIN (SELECT MAX(s.date) AS max_date FROM staging s) AS date_max
		SET r.valid_to = date_max.max_date
		WHERE NOT EXISTS (SELECT 1 FROM staging s WHERE s.object_id = r.id AND s.target_table = 'relations') 
		AND r.valid_to IS NULL;      

       
       /* VIEWS */
		/* new objects */
      INSERT INTO views(model_id, id,name,documentation,folder,valid_from,md5) 
       select s.model_id,s.object_id, s.name, s.documentation, s.parent_id, s.date, s.md5 from staging s 
       where s.target_table = 'views' AND NOT EXISTS (SELECT 1 FROM views v WHERE v.id = s.object_id);

       /* changed objects: close existing version (set valid_to) and insert new version */
      INSERT INTO views (model_id, id,name,documentation,folder,valid_from,md5) 
       SELECT s.model_id, s.object_id, s.name, s.documentation, s.parent_id, s.date, s.md5
       FROM Staging s JOIN views v ON s.object_id = v.id and s.target_table = 'views' WHERE v.valid_to is null and v.md5 != s.md5; 
      UPDATE views v JOIN Staging s ON v.id = s.object_id and v.valid_to is null and s.target_table = 'views' SET v.valid_to = s.date where v.md5 != s.md5;
       
       	/* removed objects: close existing version (set valid_to)  */ 
       UPDATE views v SET v.valid_to = (SELECT MAX(s.date) FROM Staging s)
        where v.id not IN (SELECT object_id FROM Staging s where s.target_table = 'views') 
        and v.valid_to is null;
       
             
       /* PROPERTIES */     
	   /* new properties */
       INSERT INTO properties (model_id, object_id, name, documentation, valid_from,md5)
		SELECT distinct s.model_id, s.ref_id, s.name, s.documentation, s.date,s.md5 FROM staging s
		WHERE s.target_table = 'properties' 
		AND NOT EXISTS (SELECT 1 FROM properties p WHERE p.object_id = s.ref_id and p.name=s.name);
		       
       /* verwerk aangepaste property door de oude versie af te sluiten en een nieuwe versie aan te maken */    

       INSERT INTO properties (model_id, object_id,name,documentation,valid_from,md5) SELECT distinct s.model_id, s.ref_id, s.name, s.documentation, s.date,s.md5
        FROM Staging s JOIN properties p on p.object_id = s.ref_id and p.name=s.name and p.valid_to is null WHERE p.md5 != s.md5;
       	UPDATE properties p JOIN staging s ON p.object_id = s.ref_id and p.name=s.name and p.valid_to is null SET p.valid_to = s.date WHERE p.md5 != s.md5;
       
		/* sluit verwijderde property af door valid_to in te vullen */    
		SET @max_date := (SELECT MAX(s.date) FROM staging s);
		UPDATE properties p SET p.valid_to = @max_date
		WHERE NOT EXISTS (SELECT 1 FROM staging s WHERE s.ref_id = p.object_id AND s.object_id = p.name AND s.target_table = 'properties') 
		AND p.valid_to IS NULL;

       
    /* OBJECTS_IN_VIEW */
		/* supplement nested objects; allowed relationships for nesting
		 * 1) Composition
		 * 2) Aggregation
		 * 3) Assignment
		 * 4/5) Realization/Specialization - not desirable		 */
	
	   /* 1) preferred nesting relations */
	   UPDATE staging s1
		JOIN staging s2 ON s1.source_id = s2.source_id AND s2.target_table = 'relations'
		AND s1.target_id = s2.target_id AND s2.object_type IN ('Composition', 'Aggregation', 'Assignment')
		SET s1.ref_id = s2.object_id
		WHERE s1.ref_id = 'tbd' and s1.object_type = 'nested_connection';

	   /* 2) in case nesting relation is different, then update remaining nested_connections */
	   UPDATE staging s1
		JOIN staging s2 ON s1.source_id = s2.source_id AND s2.target_table = 'relations' AND s1.target_id = s2.target_id 
		SET s1.ref_id = s2.object_id
		WHERE s1.ref_id = 'tbd' and s1.object_type = 'nested_connection';
	   
	   /* 3) in the staging file the remaining tbd's have source_id and target_id switched. This is corrected here: */
	   UPDATE staging s1
		JOIN staging s2 ON s1.source_id = s2.target_id AND s2.target_table = 'relations' AND s1.target_id = s2.source_id 
		SET s1.ref_id = s2.object_id
		WHERE s1.ref_id = 'tbd' and s1.object_type = 'nested_connection';


		/* new objects in view*/	
       INSERT INTO objects_in_view (model_id, object_id, view_id, object_type, valid_from, md5)
        SELECT distinct s.model_id, s.ref_id, s.object_id, s.object_type, s.date, md5 FROM staging s
        WHERE s.ref_id !='' and s.target_table='objects_in_view'
        and NOT EXISTS (SELECT 1 FROM objects_in_view oiv
        WHERE oiv.md5 = s.md5); -- oiv.object_id = s.ref_id AND oiv.view_id = s.object_id and oiv.valid_to is null
       
		/* objects removed from  view*/	       
       UPDATE objects_in_view oiv 
        SET oiv.valid_to = (SELECT MAX(s.date) FROM Staging s)
        WHERE NOT EXISTS (SELECT 1 FROM staging s where oiv.object_id = s.ref_id AND oiv.view_id = s.object_id and oiv.valid_to is null);
       
  
        
END
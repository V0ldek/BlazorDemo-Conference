CREATE TABLE author (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    surname VARCHAR(255) NOT NULL
);

INSERT INTO author (name, surname)
VALUES
	('Filip', 'Murlak'),
	('Krzysztof', 'Stencel'),
	('Krzysztof', 'Ciebiera'),
	('Edgar', 'Codd'),
	('Raymond', 'Boyce');

CREATE TABLE paper (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(1023) NOT NULL,
    classification VARCHAR(1023) NOT NULL
);

INSERT INTO paper (name, classification)
VALUES
	('Stackless Processing of Streamed Trees', 'Databases'),
	('A Relational Model of Data for Large Shared Data Banks', 'Databases'),
	('SEQUEL: A Structured English Query Language', 'Programming Languages'),
	('How to Match Jobs and Candidates - A Recruitment Support System Based on Feature Engineering and Advanced Analytics.', 'Recommender systems'),
	('Using genetic algorithms to optimize redundant data., Communications in Computer and Information Science', 'Databases'),
	('Universal Query Language', 'Databases');

-- SESSION is also a keyword. To tell SQL that we mean the table name,
-- put it in double quotes. Same applies for WHEN as a keyword and column name.
CREATE TABLE "session" (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "when" TIMESTAMP(0) NOT NULL, -- TIMESTAMP is the DATETIME in Postgres
                                  -- The argument is the precision of seconds.
	                          -- 0 means 0 after decimal point, so up to a second.
    chair_id INTEGER REFERENCES author NOT NULL
);

insert into "session" ("when", chair_id) values
	(TIMESTAMP '2004-10-19 10:23:54', 1),
	(TIMESTAMP '2016-07-07 10:23:54', 2),
	(TIMESTAMP '2019-01-29 10:23:54', 3),
	(TIMESTAMP '2020-11-15 10:23:54', 4);

CREATE TABLE paper_author (
	author_id INTEGER REFERENCES author NOT NULL,
	paper_id INTEGER REFERENCES paper NOT NULL,
	CONSTRAINT paper_paper_id_author_id_pkey PRIMARY KEY (paper_id, author_id)
);
select * from paper;
select * from author a ;
INSERT INTO paper_author (author_id, paper_id) VALUES
	(1, 1),
	(2, 4),
	(3, 2),
	(4, 3),
	(5, 4),
	(2, 5),
	(2, 6);

-- TRIGGER ON INSERT OR UPDATE
-- Chair of the session cannot conduct the lecture.
CREATE TABLE lecture (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "when" TIMESTAMP(0) NOT NULL,
    speaker_id INTEGER NOT NULL,
    paper_id INTEGER UNIQUE NOT NULL,
    session_id INTEGER REFERENCES session NOT NULL,
    CONSTRAINT lecture_speaker_id_session_id_key UNIQUE (speaker_id, session_id),
    CONSTRAINT lecture_speaker_id_paper_id_fkey FOREIGN KEY (speaker_id, paper_id) REFERENCES paper_author(author_id, paper_id)
);

insert into lecture ("when", speaker_id, paper_id, session_id) values 
	(TIMESTAMP '2004-10-19 10:25:54', 3, 2, 1),
	(TIMESTAMP '2016-07-07 10:26:54', 1, 1, 2),
	(TIMESTAMP '2019-01-29 10:27:54', 4, 3, 3),
	(TIMESTAMP '2020-11-15 10:29:54', 2, 4, 4);

-- Used to get sessions.
SELECT  s.id AS sessionId, s.when, 
		a.id AS authorId, a.name, a.surname, 
		l.id as lectureId, l.when, 
		a2.id as authorId, a2.name, a2.surname, 
		p.id as paperId, p.name, p.classification,
		a3.id as authorId, a3.name, a3.surname
                FROM session s
                JOIN author a
                    ON s.chair_id = a.id
				join lecture l 
					on s.id = l.session_id
				join author a2 
					on a2.id = l.speaker_id
				join paper p 
					on p.id = l.paper_id
				join paper_author pa 
					on p.id = pa.paper_id
				join author a3 
					on a3.id = pa.author_id;
                   
                  
                  
-- Triggers in Postgres must always call a special function that RETURNS TRIGGER.
-- It has access to NEW and OLD special names, that contain the NEW inserted/updated
-- row, and OLD contains the old row in case of an update. Accessing OLD during insert
-- will be a runtime error.
-- The return value will become the row to be inserted or updated.
CREATE OR REPLACE FUNCTION lecture_assert_chair_and_speaker_are_distinct_trgfn() RETURNS TRIGGER AS
$$
DECLARE chair_id INTEGER;
BEGIN
	SELECT s.chair_id INTO chair_id
	  FROM "session" s
	  WHERE id = NEW.session_id;

    IF (chair_id = NEW.speaker_id) THEN
      RAISE EXCEPTION 'Lecture with id % defines the session chair as its speaker.', NEW.id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS lecture_chair_cannot_conduct_lecture_trg ON lecture;

-- Note that unlike Oracle we are able to process the data row by row
-- and it will be completely fine.
CREATE TRIGGER lecture_chair_cannot_conduct_lecture_trg
  BEFORE INSERT OR UPDATE OF speaker_id, session_id
  ON lecture
  FOR EACH ROW
EXECUTE PROCEDURE lecture_assert_chair_and_speaker_are_distinct_trgfn();

CREATE OR REPLACE FUNCTION session_assert_chair_and_speaker_are_distinct_trgfn() RETURNS TRIGGER AS
$$
BEGIN
    IF (EXISTS (SELECT 1 FROM lecture l WHERE l.session_id = NEW.id AND l.speaker_id = NEW.chair_id)) THEN
      RAISE EXCEPTION 'Chair % is already a speaker during session %.', NEW.chair_id, NEW.id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS session_chair_cannot_conduct_lecture_trg ON lecture;

CREATE TRIGGER session_chair_cannot_conduct_lecture_trg
  BEFORE UPDATE OF id, chair_id
  ON "session"
  FOR EACH ROW
EXECUTE PROCEDURE session_assert_chair_and_speaker_are_distinct_trgfn();

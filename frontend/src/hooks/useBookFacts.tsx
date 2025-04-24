import { useEffect, useState } from "react";

const bookFacts = [
  'The first novel ever written is "The Tale of Genji", written in 11th century Japan by Murasaki Shikibu.',
  '"Green Eggs and Ham" by Dr. Seuss uses only 50 unique words.',
  'J.K. Rowling was rejected by 12 publishers before "Harry Potter and the Philosopher"s Stone" was accepted.',
  "Agatha Christie is the best-selling fiction author of all time.",
  "The Bible is the most widely distributed book in the world.",
  'The longest novel ever written is "In Search of Lost Time" by Marcel Proust, with over 1.2 million words.',
  'Charles Dickens originally published "Oliver Twist" as a serialized story.',
  'The word "robot" was first used in a 1920 play called "R.U.R." by Karel Čapek.',
  '"The Hobbit" was written by Tolkien to entertain his children.',
  '"Fahrenheit 451" is named after the temperature at which book paper catches fire.',
  'Ernest Hemingway wrote 47 different endings for "A Farewell to Arms".',
  '"Don Quixote" is considered the first modern novel.',
  "Jane Austen never published a book under her real name during her lifetime.",
  'The entire "Harry Potter" series has been translated into over 80 languages.',
  '"Ulysses" by James Joyce was banned in the U.S. until 1933.',
  "The Oxford English Dictionary took over 70 years to complete.",
  '"To Kill a Mockingbird" was the only novel published by Harper Lee for 55 years.',
  'Stephen King"s "Carrie" was almost never published—his wife rescued the manuscript from the trash.',
  'Mark Twain"s "Adventures of Huckleberry Finn" was the first novel written on a typewriter.',
  'The word "bookworm" actually predates the printing press.',
  "William Shakespeare invented over 1,700 words that are still used today.",
  "The smallest book in the world is 0.74 x 0.75 millimeters.",
  "The first book ever printed using movable type was the Gutenberg Bible in 1455.",
  '"Les Misérables" by Victor Hugo contains one of the longest sentences in literature: 823 words.',
  'George Orwell"s real name was Eric Arthur Blair.',
  '"Pride and Prejudice" was originally titled "First Impressions".',
  "Franz Kafka asked for his manuscripts to be burned after his death — thankfully, they were not.",
  '"Alice"s Adventures in Wonderland" has never been out of print since 1865.',
  'The novel "Gadsby" by Ernest Vincent Wright does not contain the letter "E".',
  'Charles Darwin"s "On the Origin of Species" sold out on its first day of publication.',
  '"A Clockwork Orange" originally had 21 chapters, but the U.S. edition omitted the last one.',
  'The original title of "1984" was "The Last Man in Europe".',
  'The world"s most stolen book is the Guinness World Records.',
  "J.R.R. Tolkien was a professor of Anglo-Saxon at Oxford University.",
  '"The Very Hungry Caterpillar" by Eric Carle has sold more than 50 million copies.',
  "George R.R. Martin writes on a DOS computer using WordStar 4.0.",
  'The longest published sentence in literature appears in "The Rotters" Club" by Jonathan Coe — 13,955 words.',
  "Roald Dahl was a fighter pilot before becoming a writer.",
  'Louisa May Alcott originally published "Little Women" under the pseudonym A.M. Barnard.',
  "The genre of science fiction was popularized by H.G. Wells and Jules Verne.",
  '"The Catcher in the Rye" was banned in many schools due to its language and themes.',
  'Emily Brontë published "Wuthering Heights" under the pen name Ellis Bell.',
  '"The Great Gatsby" sold poorly during F. Scott Fitzgerald"s lifetime.',
  '"The Alchemist" by Paulo Coelho was rejected by multiple publishers before becoming a bestseller.',
  '"Slaughterhouse-Five" by Kurt Vonnegut blends science fiction and anti-war narrative.',
  "Leo Tolstoy learned Greek at age 43 to read classics in their original language.",
  "The Library of Congress is the largest library in the world with over 170 million items.",
  "The first eBook was created in 1971 — it was the U.S. Declaration of Independence.",
  '"The Diary of Anne Frank" has been translated into more than 70 languages.',
  'Oscar Wilde once said: "If one cannot enjoy reading a book over and over again, there is no use in reading it at all."',
];

export interface TriviaResult {
  fact: string;
  next: () => void;
}

export const useBookFact = (): TriviaResult => {
  const [index, setIndex] = useState(0);

  useEffect(() => {
    next();
  }, []);

  const next = (): void => {
    if (bookFacts.length === 0) {
      return;
    }

    const randomIndex = Math.floor(Math.random() * bookFacts.length);
    setIndex(randomIndex);
  };

  return {
    fact: bookFacts[index],
    next,
  };
};

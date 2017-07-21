# CMPT 415 Proposal #

_Geoff Groos_ 

_For September 2017_

## Purpose ##
---

>The purpose of this self directed study is to explore strategies for implementing a Constrained Random parameter-Value-set Generator (CRVG)

## Whereas ##
---

- Nick Sumner has substantial expertise in the area of symbolic execution for generation of data similar to this.
- Geoff Groos is the student, in good standing at SFU, with interest in optimization and related technologies.

## Background ##
---

There are many applications for random numbers in the field of optimization. One common area that optimization is applied is to design work, wherein a particular system or design is modelled in a Computer Aided Engineering (CAE) software, and then optimized. These designs are often parameterized on key elements. By choosing different values for a particular parameterized design, different solutions can be created. Choosing values for these parameters is often quite difficult, particularly if it is automated, as there can be an extensive set of arbitrary constraints on those values.

A software-driven optimization of this design would look like this:

1. a designer or engineer models his problem in a CAE software to produce a design.
2. the designer or engineer then introduces parameters on his design in the CAE software, such that a 3rd party can pick values to fill those parameters. Some common parameters might be the length or quality of steel used by a particular component.
3. The optimization software picks values for the parameters on the design to produce a new solution.
4. the CAE software analyzes the new solution, and produces an output report about its performance or quality, typically describing things like its cost, safety-factor, speed, etc.
5. based on this output, the optimization software selects new parameter values for the design, and jumps back to step 3.

To perform step 3, the optimization software must generate _feasible solutions_, that is, it must generate values for the parameters that will actually lead to real potential solutions. It cannot, for example, generate a component with a negative length. If one can express safety as one or more constraints then the optimization software should only produce parameter values that yield solutions passing all safety constraints.

In this way, for the optimization software to operate, it must build a fair comprehension of what parameter values lead to feasible solutions and what parameter values lead to infeasible solutions. The typical strategy for doing this is to survey the input space randomly, evaluating many possible parameter value sets, and picking from those evaluations only the ones that are feasible to move forward in the optimization. If the input space is sampled randomly then the resulting designs will be evenly distributed across the entire input space. However, if the feasible space is much smaller than the total input space, random sampling is often inadequate as it will not be able to generate enough feasible solutions.

Because this constraint comprehension is happening before any optimization, it must be done fairly. If, for example, the constraint comprehension system generates 100 solutions that are all feasible but all roughly the same, and one solution that is also feasible but very different, then the optimization will likely be skewed in favor of solutions in the group of 100 similar solutions. Thus it is important that any system not using the simple random sampling approach use an approach that produces near-uniformly-distributed feasible solutions. 

### A turbine engine example ###

   - For example, if one was modelling a turbine engine, they might use a mechanical simulation software like ANSYS or Turbo-Tides. A common set of parameters for a particular turbine engine design might be the length, width, and number of the turbine's compressor blades, and the speed at which they will rotate. In this situation, the typical output to maximized in the optimization is the compression ratio, a value closely related to the engine's performance.

  Thus we might have a design with the parameters:

     - blade_width
     - blade_height
     - blade_count
     - speed

  One constraint we might add to these designs is that the mass of the blades is not to exceed some fixed limit. If we assume that the mass of each compressor blade is `blade_width * blade_height * 1.0mm * 0.123kg-per-cubic-mm`, and that the total mass of the system is then simply `blade_width * blade_height * 1.0 * 0.123 * blade_count`, and we didn't want the overall mass to exceed 2kg, then our constraint could be written as

   ```
   mass_constraint: blade_width * blade_height * 1.0 * 0.123 < 2.0
   ```

In actual designs, there may be many such constraints which must be satisfied for the parameter values to form a feasible solution.

In this sense the optimization software must employ a _constrained random parameter-value-set generator_. This generator must produce parameter value sets that: 

1. lead to solutions that pass all constraints ("feasible" solutions)
2. are evenly distributed in the space of all possible parameter value sets. 


## Topics ##
---

There are 4 major components to the CRVG

### 1. Random "Guess & Check" ###

One strategy to solving a system of complex inequalities is simply to generate a completely random vector evenly distributed across the entire real number space and check that vector against each constraint. This solution has the advantage of being perfectly uniform, but often fails to discover feasible regions.

The first step in this project will be to (quickly) build a system to generate completely random vectors in the input space and check those vectors against the set of constraints. This will then provide a baseline for which further solutions can be benchmarked. 

### 2. Quality measurement system ###

One critical aspect to measuring success of the system will be to determine the quality of the output solutions. Given a predetermined set of constraints, this component will evaluate any CRVG implementation and provide a summary of its effectiveness. The key quality metrics are:

- Did it produce a sufficient number of solutions that satisfy all constraints?
- Did it find all of the feasible regions?
- Was its distribution within those regions uniform? In other words, did it successfully find points closer to the extrema of the region?

Any stochastic behaviour in CRVG implementations should be accounted for either by using a fixed-seed for the random number generator or by using a sufficient number of trials to make the benchmark provide reasonably consistent results. 

### 3. Symbolic-Execution Driven Approximation ###

Symbolic execution is a strategy that can help partition the input space such that it more closely matches the feasible region. By performing a special evaluation of the constraint expressions, and using heuristics to arbitrarily reduce the space at key points, symbolic execution can drive the generator toward feasible regions. 

By tracking dependencies between variables and the range that each part of an expression can produce, symbolic execution builds data structures that can encapsulate the feasible space into a group of ranges that each variable might take on to produce a feasible solution. 

### 4.Feasible point exploration ###

Given a feasible point has been found, another strategy to produce more feasible solutions is to search for an approximation of the bounding region that this particular feasible solution lies in. 

Additional points can be found by starting from a seed point, picking a random direction, applying a binary search to find the edge of the feasible region in that direction, and then picking a random point between the two resulting points. 

## Schedule ##
---

This project is expected to take the duration of the fall semester, starting on September 5 and ending on December 4.

The work will be done in one week increments, with a brief online meeting each week to review what was done and new goals for the next week, with the supervisor.

## Success/Grading Criteria ##
---

Every two weeks a written status update will be submitted for grading. This status update will include a description of any research done into a technique, in addition to any attempted implementation of that technique. Each of these 6 reports is worth 10% of the final grade.

At the end of a semester, an aggregate report of the various techniques tried, and their relative strengths and weaknesses will be submitted for grading. This will serve as a term project, worth 40% of the grade. 
, 
